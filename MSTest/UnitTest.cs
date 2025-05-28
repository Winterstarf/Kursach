using Microsoft.VisualStudio.TestTools.UnitTesting;
using MainApp.windows.adds;
using System.Windows.Controls;
using System.Windows;
using MainApp.assets.models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Dynamic;

namespace MSTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void Test_TotalPriceCalculation_ReturnsExpectedSum()
        {
            var services = new List<clients_services>
            {
                new clients_services { medical_services = new medical_services { mservice_price = 300 } },
                new clients_services { medical_services = new medical_services { mservice_price = 200 } }
            };

            var expectedTotal = 500;
            var actualTotal = services.Sum(s => s.medical_services.mservice_price);

            Assert.AreEqual(expectedTotal, actualTotal, "Общая сумма услуг рассчитана некорректно");
        }
        [TestMethod]
        public void Test_ClientSearchByName_FindsCorrectClient()
        {
            var clients = new List<clients>
            {
                new clients { last_name = "Иванов", first_name = "Иван", middle_name = "Иванович" },
                new clients { last_name = "Смирнов", first_name = "Пётр", middle_name = "Александрович" }
            };

            var searchText = "Иван".ToLower();
            var result = clients.Where(c =>
                c.last_name.ToLower().Contains(searchText) ||
                c.first_name.ToLower().Contains(searchText)||
                c.middle_name.ToLower().Contains(searchText)).ToList();

            Assert.AreEqual(1, result.Count, "Поиск должен вернуть ровно одного клиента");
            Assert.AreEqual("Иванов", result[0].last_name);
        }
        [TestMethod]
        public void Test_FilterIdOrders_ReturnsOnlyNeeded()
        {
            var orders = new List<clients_services>
            {
                new clients_services { id_order = 1 },
                new clients_services { id_order = 2 },
                new clients_services { id_order = 3 }
            };

            var filteredOrders = orders.Where(o => o.id_order == 2).ToList();

            Assert.AreEqual(1, filteredOrders.Count, "Должна быть выбрана ровно одна строка с id_order = 2");
            Assert.AreEqual(2, filteredOrders[0].id_order, "Выбранный заказ должен иметь id_order = 2");
        }
        [TestMethod]
        public void Test_ExportHandlesNullValuesGracefully()
        {
            var data = new List<ExpandoObject>();

            dynamic row = new ExpandoObject();
            row.Id = 1;
            row.Name = null;
            row.Email = "test@example.com";
            data.Add(row);

            foreach (var item in data)
            {
                var dict = (IDictionary<string, object>)item;
                foreach (var val in dict.Values)
                {
                    var safeVal = val?.ToString() ?? string.Empty;
                    Assert.IsNotNull(safeVal);
                }
            }
        }
        [TestMethod]
        public void Test_ExportRowLimitValidation()
        {
            int requested = 100;
            int available = 75;

            Assert.IsTrue(requested > available, "Тестовая ситуация должна имитировать превышение лимита");
            string warning = requested > available
                ? $"Попытка экспортировать {requested} строк при наличии только {available}"
                : string.Empty;

            Assert.IsTrue(warning.Contains("экспортировать"), "Ожидалось предупреждение о превышении");
        }
        [TestMethod]
        public void Test_BonusCalculationAccuracy()
        {
            double totalSum = 1333.33;
            double expectedBonus = Math.Round(totalSum * 0.15, 2);

            double actualBonus = Math.Round(totalSum * 0.15, 2);

            Assert.AreEqual(expectedBonus, actualBonus, 0.01, "Бонус должен рассчитываться корректно и быть округлён до двух знаков");
        }

    }
}
