﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Services.PriceHistoryService;
using BLL.Services.ProductService;
using Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NotificationApp.Interfaces;
using NUnit.Framework;
using Messages;
using NotificationApp.JobTask;

namespace NotificationAppTests
{
    [TestClass]
    public class PriceComparisonTests
    {
        [TestMethod]
        [TestCase(12, 12)]
        [TestCase(0, 12)]
        [TestCase(12, 11)]
        [TestCase(12, 13)]
        [TestCase(12, 0)]
        public async Task ProductPriceTests(double priceFromDb, double priceFromSite)
        {
            //Arrange
            double parsedPrice = priceFromSite;
            var userId = 11;

            var user = new User
            {
                Id = userId,
                SocialNetworkName = "Twitter",
                Username = "Stan",
                SocialNetworkUserId = "297397558",
                Token = "4f60b211517aa86a67bace12231d2530",
                Email = "stanislav.soloviyev@yandex.ru"
            };

            List<UserProduct> userProducts = new List<UserProduct>
            {
                new UserProduct { Checked = true, ProductId = 33, UserId = 11,User = user }
            };

            List<ProvidersProductInfo> providersProductInfos = new List<ProvidersProductInfo>
            {
                new ProvidersProductInfo {ImageUrl = "asd",MinPrice = priceFromDb, MaxPrice = 16, ProviderName = "Onliner", Id = 1, Url = "qwe"}
            };

            List<Product> products = new List<Product>()
            {
                new Product
                {
                Id = 33,
                ExternalProductId = "12345",
                Name = "asdasasf",
                ProvidersProductInfos = providersProductInfos,
                UserProducts = userProducts
                }
            };

            var mockProductService = new Mock<IProductService>();
            var mockExternalProductService = new Mock<IExternalProductService>();
            var mockMailService = new Mock<IMailService>();
            var mockPriceHistoryService = new Mock<IPriceHistoryService>();
            var mockMessageService = new Mock<IMessageService>();
            mockProductService.Setup(x => x.GetTrackedItems()).ReturnsAsync(products).Verifiable();
            if (priceFromDb != priceFromSite)
            {
                mockProductService.Setup(x => x.Update(It.IsAny<Product>())).Returns(Task.FromResult(false)).Verifiable();
                mockMessageService.Setup(c => c.SendPriceUpdate(It.IsAny<UpdatedPricesMessage>())).Verifiable();
            }
            mockExternalProductService.Setup(x => x.ParsePrice(It.IsAny<string>())).ReturnsAsync(parsedPrice).Verifiable();
            mockMailService.Setup(x => x.ProductAvailable(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>())).Returns(Task.FromResult(false)).Verifiable();

            var priceComparisonJob = new PriceComparisonJob
                (
                    mockProductService.Object,
                    mockExternalProductService.Object,
                    mockMailService.Object,
                    mockPriceHistoryService.Object,
                    mockMessageService.Object
                );

            //Act
            await priceComparisonJob.Compare();

            //Assert
            mockProductService.Verify();
            mockExternalProductService.Verify();
        }
    }
}
