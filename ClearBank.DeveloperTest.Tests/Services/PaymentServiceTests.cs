using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Moq;
using NUnit.Framework;

namespace ClearBank.DeveloperTest.Tests.Services
{
    [TestFixture]
    public class PaymentServiceTests
    {
        private readonly Mock<IAccountDataStore> _accountDataStore;
        private readonly PaymentService paymentService;

        public PaymentServiceTests()
        {
            _accountDataStore = new Mock<IAccountDataStore>();
            paymentService = new PaymentService(_accountDataStore.Object);
        }
        
        [Test]
        public void MakePayment_When_accountIsNull_Then_ReturnsFailure()
        {
            // setup
            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns((Account)null);

            // act
            var res = paymentService.MakePayment(new MakePaymentRequest());

            // asserts
            Assert.False(res.Success);
        }

        [Test]
        public void MakePayment_When_PaymentSchemeIsBacsAndAllowed_Then_ReturnsSuccess()
        {
            // setup
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs,
                Balance = 110
            };

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);
            _accountDataStore.Setup(x => x.UpdateAccount(It.IsAny<Account>())).Verifiable();

            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.Bacs,
                Amount = 50
            };

            // act
            var res = paymentService.MakePayment(request);

            // asserts
            Assert.True(res.Success);
            Assert.AreEqual(60, account.Balance);
            _accountDataStore.Verify(x => x.UpdateAccount(account), Times.Once);
        }

        [Test]
        public void MakePayment_When_PaymentSchemeIsBacsAndNotAllowed_Then_ReturnsFailure()
        {
            // setup
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps,
                Balance = 110
            };

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);
            _accountDataStore.Setup(x => x.UpdateAccount(It.IsAny<Account>())).Verifiable();

            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.Bacs,
                Amount = 50
            };

            // act
            var res = paymentService.MakePayment(request);

            // asserts
            Assert.False(res.Success);
            Assert.AreEqual(110, account.Balance);
            _accountDataStore.Verify(x => x.UpdateAccount(account), Times.Never);
        }

        [Test]
        public void MakePayment_When_PaymentSchemeIsFasterPaymentAndAllowedAndBalanceIsGreaterThanAmount_Then_ReturnsSuccess()
        {
            // setup
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Balance = 110
            };

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);
            _accountDataStore.Setup(x => x.UpdateAccount(It.IsAny<Account>())).Verifiable();

            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.FasterPayments,
                Amount = 50
            };

            // act
            var res = paymentService.MakePayment(request);

            // asserts
            Assert.True(res.Success);
            Assert.AreEqual(60, account.Balance);
            _accountDataStore.Verify(x => x.UpdateAccount(account), Times.Once);

        }

        [Test]
        public void MakePayment_When_PaymentSchemeIsFasterPaymentAndNotAllowedAndBalanceIsGreaterThanAmount_Then_ReturnsFailure()
        {
            // setup
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Balance = 110
            };

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);
            _accountDataStore.Setup(x => x.UpdateAccount(It.IsAny<Account>())).Verifiable();

            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.Chaps,
                Amount = 50
            };

            // act
            var res = paymentService.MakePayment(request);

            // asserts
            Assert.False(res.Success);
            Assert.AreEqual(110, account.Balance);
            _accountDataStore.Verify(x => x.UpdateAccount(account), Times.Never);
        }

        [Test]
        public void MakePayment_When_PaymentSchemeIsFasterPaymentAndAllowedAndBalanceIsLowerThanAmount_Then_ReturnsFailure()
        {
            // setup
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Balance = 40
            };

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);
            _accountDataStore.Setup(x => x.UpdateAccount(It.IsAny<Account>())).Verifiable();

            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.FasterPayments,
                Amount = 50
            };

            // act
            var res = paymentService.MakePayment(request);

            // asserts
            Assert.False(res.Success);
            Assert.AreEqual(40, account.Balance);
            _accountDataStore.Verify(x => x.UpdateAccount(account), Times.Never);
        }

        [Test]
        public void MakePayment_When_PaymentSchemeIsChapsAndAllowedAndAccountStatusIsLive_Then_ReturnsSuccess()
        {
            // setup
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps,
                Balance = 110
            };

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);
            _accountDataStore.Setup(x => x.UpdateAccount(It.IsAny<Account>())).Verifiable();

            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.Chaps,
                Amount = 50
            };

            // act
            var res = paymentService.MakePayment(request);

            // asserts
            Assert.True(res.Success);
            Assert.AreEqual(60, account.Balance);
            _accountDataStore.Verify(x => x.UpdateAccount(account), Times.Once);
        }

        [Test]
        public void MakePayment_When_PaymentSchemeIsChapsAndNotAllowedAndAccountStatusIsLive_Then_ReturnsFailure()
        {
            // setup
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Balance = 110,
                Status = AccountStatus.Live
            };

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);
            _accountDataStore.Setup(x => x.UpdateAccount(It.IsAny<Account>())).Verifiable();

            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.Chaps,
                Amount = 50
            };

            // act
            var res = paymentService.MakePayment(request);

            // asserts
            Assert.False(res.Success);
            Assert.AreEqual(110, account.Balance);
            _accountDataStore.Verify(x => x.UpdateAccount(account), Times.Never);
        }

        [Test]
        public void MakePayment_When_PaymentSchemeIsChapsAndAllowedAndAccountStatusIsNotLive_Then_ReturnsFailure()
        {
            // setup
            var account = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Balance = 110,
                Status = AccountStatus.Disabled
            };

            _accountDataStore.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);
            _accountDataStore.Setup(x => x.UpdateAccount(It.IsAny<Account>())).Verifiable();

            var request = new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.Chaps,
                Amount = 50
            };

            // act
            var res = paymentService.MakePayment(request);

            // asserts
            Assert.False(res.Success);
            Assert.AreEqual(110, account.Balance);
            _accountDataStore.Verify(x => x.UpdateAccount(account), Times.Never);
        }
    }
}
