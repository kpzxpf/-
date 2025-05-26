using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MedCentre.dto;
using MedCentre.models;
using MedCentre.service;
using Microsoft.Win32;

namespace MedCentre
{
    public partial class OrderWindow : Window, INotifyPropertyChanged
    {
        private OrderViewModel _orderViewModel;
        private OrderManager _orderManager;
        private OrderService _orderService;
        private UserSession _userSession;

        public OrderViewModel OrderViewModel
        {
            get => _orderViewModel;
            set
            {
                if (_orderViewModel != value)
                {
                    _orderViewModel = value;
                    OnPropertyChanged(nameof(OrderViewModel));
                }
            }
        }

        public OrderWindow()
        {
            InitializeComponent();
            
            _orderManager = OrderManager.Instance;
            _orderService = new OrderService();
            _userSession = UserSession.Instance;
            
            InitializeOrderViewModel();
            DataContext = _orderViewModel;
        }

        private void InitializeOrderViewModel()
        {
            _orderViewModel = new OrderViewModel
            {
                OrderNumber = _orderService.GetNextOrderNumber(),
                OrderDate = DateTime.Now,
                CustomerName = _userSession.CurrentUser?.Name ?? "Неизвестный клиент",
                PickupPoint = "Адрес",
                PickupCode = _orderService.GeneratePickupCode(),
                DeliveryDays = _orderService.CalculateDeliveryDays(_orderManager.CurrentOrderItems.ToList()),
                OrderItems = _orderManager.CurrentOrderItems
            };
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                var result = MessageBox.Show("Удалить товар из заказа?", 
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _orderManager.RemoveProduct(productId);
                    
                    if (!_orderManager.HasItems)
                    {
                        MessageBox.Show("Заказ пуст. Окно будет закрыто.", 
                            "Заказ пуст", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                }
            }
        }

        private async void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_orderManager.HasItems)
                {
                    MessageBox.Show("Заказ пуст", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var itemsToRemove = new List<OrderItemViewModel>();
                foreach (var item in _orderManager.CurrentOrderItems)
                {
                    if (item.Quantity <= 0)
                    {
                        itemsToRemove.Add(item);
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    _orderManager.RemoveProduct(item.ProductId);
                }

                if (!_orderManager.HasItems)
                {
                    MessageBox.Show("Заказ пуст после удаления товаров с нулевым количеством", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var order = await _orderService.CreateOrderAsync(
                    _userSession.CurrentUser.Id, 
                    _orderManager.CurrentOrderItems.ToList());

                MessageBox.Show($"Заказ №{order.Id} успешно создан!", 
                    "Заказ создан", MessageBoxButton.OK, MessageBoxImage.Information);

                GenerateReceipt();

                _orderManager.ClearOrder();
                
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveReceipt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Талон_заказ_{_orderViewModel.OrderNumber}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    GenerateReceiptPDF(saveFileDialog.FileName);
                    MessageBox.Show("Талон сохранен успешно!", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении талона: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateReceipt()
        {
            try
            {
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                    $"Талон_заказ_{_orderViewModel.OrderNumber}.pdf");
                GenerateReceiptPDF(fileName);
                
                var result = MessageBox.Show($"Талон сохранен на рабочий стол: {fileName}\n\nОткрыть файл?", 
                    "Талон создан", MessageBoxButton.YesNo, MessageBoxImage.Information);
                
                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = fileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании талона: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       private void GenerateReceiptPDF(string fileName)
        {
        Document document = new Document();
        PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
        
        document.Open();
        
        BaseFont baseFont = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
        Font titleFont = new Font(baseFont, 18, Font.BOLD);
        Font headerFont = new Font(baseFont, 14, Font.BOLD);
        Font normalFont = new Font(baseFont, 12, Font.NORMAL);
        Font codeFont = new Font(baseFont, 16, Font.BOLD);
    
        Paragraph title = new Paragraph("ТАЛОН ЗАКАЗА", titleFont);
        title.Alignment = Element.ALIGN_CENTER;
        title.SpacingAfter = 20f;
        document.Add(title);
    
        document.Add(new Paragraph($"Дата заказа: {_orderViewModel.OrderDate:dd.MM.yyyy}", normalFont));
        document.Add(new Paragraph($"Номер заказа: {_orderViewModel.OrderNumber}", normalFont));
        document.Add(new Paragraph($"Клиент: {_orderViewModel.CustomerName}", normalFont));
        document.Add(new Paragraph(" ", normalFont));
    
        Paragraph orderHeader = new Paragraph("СОСТАВ ЗАКАЗА:", headerFont);
        orderHeader.SpacingBefore = 10f;
        orderHeader.SpacingAfter = 10f;
        document.Add(orderHeader);
    
        PdfPTable table = new PdfPTable(4);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 3, 1, 2, 2 });
    
        AddTableCell(table, "Наименование", headerFont, true);
        AddTableCell(table, "Кол-во", headerFont, true);
        AddTableCell(table, "Цена", headerFont, true);
        AddTableCell(table, "Сумма", headerFont, true);
    
        foreach (var item in _orderViewModel.OrderItems)
        {
            AddTableCell(table, $"{item.ProductName} ({item.Brand})", normalFont);
            AddTableCell(table, item.Quantity.ToString(), normalFont);
            AddTableCell(table, $"{item.DiscountedPrice:N2} руб.", normalFont);
            AddTableCell(table, $"{item.TotalPrice:N2} руб.", normalFont);
        }
    
        document.Add(table);
    
        document.Add(new Paragraph(" ", normalFont));
        document.Add(new Paragraph($"Общая сумма: {_orderViewModel.TotalAmount:N2} руб.", normalFont));
        document.Add(new Paragraph($"Размер скидки: {_orderViewModel.DiscountAmount:N2} руб.", normalFont));
        
        Paragraph totalPara = new Paragraph($"К ОПЛАТЕ: {_orderViewModel.FinalAmount:N2} руб.", headerFont);
        totalPara.SpacingBefore = 5f;
        totalPara.SpacingAfter = 15f;
        document.Add(totalPara);
    
        document.Add(new Paragraph($"Пункт выдачи: {_orderViewModel.PickupPoint}", normalFont));
        document.Add(new Paragraph($"Срок доставки: {_orderViewModel.DeliveryDays} дней", normalFont));
        
        Paragraph codePara = new Paragraph($"КОД ПОЛУЧЕНИЯ: {_orderViewModel.PickupCode}", codeFont);
        codePara.Alignment = Element.ALIGN_CENTER;
        codePara.SpacingBefore = 20f;
        codePara.SpacingAfter = 10f;
        document.Add(codePara);
    
        document.Close(); 
        }

        private void AddTableCell(PdfPTable table, string text, Font font, bool isHeader = false)
        {
        PdfPCell cell = new PdfPCell(new Phrase(text, font));
        cell.Padding = 5f;
        cell.HorizontalAlignment = Element.ALIGN_CENTER;
    
        if (isHeader)
        {
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        }
    
        table.AddCell(cell);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}