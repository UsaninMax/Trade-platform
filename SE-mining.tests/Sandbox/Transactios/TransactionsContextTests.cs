﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using SEMining.Sandbox.Transactios;
using SE_mining_base.Sandbox.Models;
using SE_mining_base.Transactios.Enums;
using SE_mining_base.Transactios.Models;

namespace SEMining.tests.Sandbox.Transactios
{
    [TestFixture]
    public class TransactionsContextTests
    {
        private Mock<IWorkingPeriodHolder> _workingPeriodHolderMock;
        private Mock<ITransactionBuilder> _transactionBuilderMock;
        private Mock<ITransactionHolder> _transactionHolderMock;
        private Mock<IBalanceHolder> _balanceMock;

        [SetUp]
        public void SetUp()
        {
            _workingPeriodHolderMock = new Mock<IWorkingPeriodHolder>();
            ContainerBuilder.Container.RegisterInstance(_workingPeriodHolderMock.Object);
            _transactionBuilderMock = new Mock<ITransactionBuilder>();
            ContainerBuilder.Container.RegisterInstance(_transactionBuilderMock.Object);
            _transactionHolderMock = new Mock<ITransactionHolder>();
            ContainerBuilder.Container.RegisterInstance(_transactionHolderMock.Object);
            _balanceMock = new Mock<IBalanceHolder>();
            ContainerBuilder.Container.RegisterInstance(_balanceMock.Object);
        }


        [Test]
        public void Check_reset()
        {
            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>());
            context.Reset();
            _transactionBuilderMock.Verify(x=> x.Reset(), Times.Once);
            _transactionHolderMock.Verify(x => x.Reset(), Times.Once);
            _balanceMock.Verify(x => x.Reset(), Times.Once);
        }

        [Test]
        public void Check_if_context_is_prepared()
        {
            _balanceMock.Setup(x => x.GetHistory()).Returns(new List<BalanceRow>() { new BalanceRow.Builder().Build() });

            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "instrument_id", new BrokerCost()}
            });

            context.SetUpBalance(1000);
            _balanceMock.Verify(x => x.AddMoney(1000), Times.Once);

            Assert.That(context.IsPrepared, Is.True);
        }

        [Test]
        public void Check_available_number_if_context_not_set_up()
        {
            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "instrument_id", new BrokerCost()}
            });
            context.ProcessTick(new Dictionary<string, Tick>()
            {
                {"test_id", new Tick.Builder().WithId("test_id").WithPrice(120).Build()}
            }, DateTime.MinValue);

            Assert.Throws<Exception>(() =>
            {
                context.GetAvailableNumberToOpen("test");
            });
        }

        [Test]
        public void Calculate_available_number()
        {
            _balanceMock.Setup(x => x.GetTotal()).Returns(1000);

            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "test_id", new BrokerCost{Coverage = 0.10, TransactionCost = 2}},
                { "test_id_2", new BrokerCost{Coverage = 0.20, TransactionCost = 4}}
            });

            IDictionary<string, Tick> ticks = new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithId("test_id").WithPrice(120).Build()},
                {"test_id_2", new Tick.Builder().WithId("test_id_2").WithPrice(113).Build()}
            };
            context.ProcessTick(ticks, DateTime.MinValue);
            context.SetUpBalance(1000);

            Assert.That(context.GetAvailableNumberToOpen("test_id"), Is.EqualTo(83));

            _transactionHolderMock.Setup(x => x.GetCoverage(ticks, It.IsAny<IEnumerable<OpenPositionRequest>>())).Returns(103);
            Assert.That(context.GetAvailableNumberToOpen("test_id"), Is.EqualTo(74));
        }

        [Test]
        public void Open_position_when_not_tick_processed()
        {
            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "test_id", new BrokerCost{Coverage = 0.10, TransactionCost = 2}}
            });

            OpenPositionRequest request = new OpenPositionRequest.Builder().Build();
            Assert.That(context.OpenPosition(request), Is.False);
        }

        [Test]
        public void Open_position_when_not_set_up_workin_time()
        {
            _balanceMock.Setup(x => x.GetTotal()).Returns(1000);

            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "test_id", new BrokerCost(){ Coverage = 0.10, TransactionCost = 2}}
            });


            context.ProcessTick(new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithId("test_id").WithPrice(120).Build()}
            }, DateTime.MinValue);

            OpenPositionRequest request = new OpenPositionRequest.Builder()
                .InstrumentId("test_id")
                .Number(10)
                .Direction(Direction.Buy)
                .Build();
            Assert.That(context.OpenPosition(request), Is.True);
        }

        [Test]
        public void Open_position_when_set_up_workin_time()
        {
            _balanceMock.Setup(x => x.GetTotal()).Returns(1000);
            _transactionHolderMock.Setup(x => x.GetOpenTransactions(It.IsAny<string>()))
                .Returns(new List<Transaction>());
            _workingPeriodHolderMock.Setup(x => x.Get("test_id"))
                .Returns(new WorkingPeriod {Open = new TimeSpan(0, 10, 30, 0), Close = new TimeSpan(0, 20, 0, 0)});
            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "test_id", new BrokerCost(){Coverage = 0.10, TransactionCost = 2}}
            });


            context.ProcessTick(new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithDate(new DateTime(2016, 9, 12, 6, 45, 0)).WithId("test_id").WithPrice(120).Build()}
            }, new DateTime(2016, 9, 12, 6, 45, 0));

            context.SetUpBalance(1000);

            Assert.That(context.OpenPosition(new OpenPositionRequest.Builder()
                .InstrumentId("test_id")
                .Number(10)
                .Direction(Direction.Buy)
                .Build()), Is.False);

            context.ProcessTick(new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithDate(new DateTime(2016, 9, 12, 10, 30, 1)).WithId("test_id").WithPrice(120).Build()}
            }, new DateTime(2016, 9, 12, 10, 30, 1));

            Assert.That(context.OpenPosition(new OpenPositionRequest.Builder()
                .InstrumentId("test_id")
                .Number(10)
                .Direction(Direction.Buy)
                .Build()), Is.True);

            context.ProcessTick(new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithDate(new DateTime(2016, 9, 12, 20, 0, 1)).WithId("test_id").WithPrice(120).Build()}
            }, new DateTime(2016, 9, 12, 20, 0, 1));

            Assert.That(context.OpenPosition(new OpenPositionRequest.Builder()
                .InstrumentId("test_id")
                .Number(10)
                .Direction(Direction.Buy)
                .Build()), Is.False);

            context.ProcessTick(new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithDate(new DateTime(2016, 9, 12, 19, 59, 59)).WithId("test_id").WithPrice(120).Build()}
            }, new DateTime(2016, 9, 12, 19, 59, 59));

            Assert.That(context.OpenPosition(new OpenPositionRequest.Builder()
                .InstrumentId("test_id")
                .Number(10)
                .Direction(Direction.Buy)
                .Build()), Is.True);
        }


        [Test]
        public void Open_position_check_history()
        {
            _balanceMock.Setup(x => x.GetTotal()).Returns(1000);

            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "test_id", new BrokerCost(){Coverage = 0.10, TransactionCost = 2}}
            });

            context.ProcessTick(new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithDate(new DateTime(2016, 9, 12, 6, 45, 0)).WithId("test_id").WithPrice(120).Build()}
            }, new DateTime(2016, 9, 12, 6, 45, 0));

            context.SetUpBalance(1000);

            OpenPositionRequest request = new OpenPositionRequest.Builder()
                .InstrumentId("test_id")
                .Number(10)
                .Direction(Direction.Buy)
                .Build();

            Assert.That(context.OpenPosition(request), Is.True);
            Assert.That(context.GetActiveRequests().Count, Is.EqualTo(1));
            Assert.That(context.GetRequestsHistory().Count, Is.EqualTo(1));
            Assert.That(context.GetActiveRequests().First(), Is.EqualTo(request));
            Assert.That(context.GetRequestsHistory().First(), Is.EqualTo(request));
        }

        [Test]
        public void Open_position_check()
        {
            _balanceMock.Setup(x => x.GetTotal()).Returns(1000);

            IDictionary<string, Tick> tick = new Dictionary<string, Tick>
            {
                {
                    "test_id", new Tick.Builder()
                        .WithDate(new DateTime(2016, 9, 12, 11, 46, 0))
                        .WithId("test_id")
                        .WithPrice(160)
                        .WithVolume(80)
                        .Build()
                }
            };

            OpenPositionRequest request = new OpenPositionRequest.Builder()
                .InstrumentId("test_id")
                .Number(10)
                .Direction(Direction.Buy)
                .Build();

            IEnumerable<Transaction> transactions = new List<Transaction>
            {
                new Transaction.Builder().InstrumentId("test_id").Build()
            };

            _transactionHolderMock.Setup(x => x.GetInvertedOpenTransactions("test_id", Direction.Buy)).Returns(transactions);

            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "test_id", new BrokerCost{ Coverage = 0.10, TransactionCost = 2}}
            });


            context.ProcessTick(new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithDate(new DateTime(2016, 9, 12,11, 45, 0)).WithId("test_id").WithPrice(120).Build()}
            }, new DateTime(2016, 9, 12, 11, 45, 0));

            Assert.That(context.OpenPosition(request), Is.True);
            Assert.That(context.GetActiveRequests().Count, Is.EqualTo(1));
            Assert.That(context.GetRequestsHistory().Count, Is.EqualTo(1));
            Assert.That(context.GetActiveRequests().First().Date, Is.EqualTo(new DateTime(2016, 9, 12, 11, 45, 0)));

            Transaction transaction = new Transaction.Builder().InstrumentId("test_id").Direction(Direction.Buy)
                .Number(10).Build();

            _transactionBuilderMock.Setup(x => x.Build(request, tick["test_id"])).Returns(transaction);
            context.ProcessTick(tick, new DateTime(2016, 9, 12, 11, 46, 0));

            _balanceMock.Verify(x => x.AddTransactionMargin(transaction, transactions, It.IsAny<DateTime>()), Times.Once);
            _transactionHolderMock.Verify(x => x.UpdateOpenTransactions(transaction), Times.Once);
            Assert.That(context.GetActiveRequests().Count, Is.EqualTo(0));
            Assert.That(context.GetRequestsHistory().Count, Is.EqualTo(1));
        }



        [Test]
        public void Force_close_positions()
        {
          
            _balanceMock.Setup(x => x.GetTotal()).Returns(1000);


            _workingPeriodHolderMock.Setup(x => x.Get("test_id"))
                .Returns(new WorkingPeriod {Open = new TimeSpan(0, 10, 30, 0), Close = new TimeSpan(0, 20, 0, 0)});


            IDictionary<string, Tick> tick = new Dictionary<string, Tick>
            {
                {
                    "test_id", new Tick.Builder()
                        .WithDate(new DateTime(2016, 9, 12, 20, 46, 0))
                        .WithId("test_id")
                        .WithPrice(160)
                        .WithVolume(80)
                        .Build()
                }
            };

            OpenPositionRequest request = new OpenPositionRequest.Builder()
                .InstrumentId("test_id")
                .Number(10)
                .Direction(Direction.Buy)
                .Build();

            IEnumerable<Transaction> transactions = new List<Transaction>
            {
                new Transaction.Builder().InstrumentId("test_id").Direction(Direction.Buy).Number(20).ExecutedPrice(123).Build(),
                new Transaction.Builder().InstrumentId("test_id").Direction(Direction.Buy).Number(40).ExecutedPrice(123).Build(),
                new Transaction.Builder().InstrumentId("test_id").Direction(Direction.Buy).Number(60).ExecutedPrice(123).Build(),
                new Transaction.Builder().InstrumentId("test_id").Direction(Direction.Sell).Number(10).ExecutedPrice(123).Build()
            };

            _transactionHolderMock.Setup(x => x.GetOpenTransactions("test_id")).Returns(transactions);

            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "test_id", new BrokerCost{ Coverage = 0.10, TransactionCost = 2}}
            });

            context.ProcessTick(new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithDate(new DateTime(2016, 9, 12,11, 45, 0)).WithId("test_id").WithPrice(120).Build()}
            }, new DateTime(2016, 9, 12, 11, 45, 0));

            Assert.That(context.OpenPosition(request), Is.True);

            context.ProcessTick(tick, new DateTime(2016, 9, 12, 20, 46, 0));
            _balanceMock.Verify(x => x.AddTransactionMargin(It.IsAny<Transaction>(), transactions, DateTime.MinValue), Times.Never);
            _balanceMock.Verify(x => x.AddTransactionCost(2, It.IsAny<DateTime>(), It.IsAny<Guid>()), Times.Exactly(3));
            _transactionHolderMock.Verify(x => x.UpdateOpenTransactions(It.IsAny<Transaction>()), Times.Never);
            Assert.That(context.GetActiveRequests().Count, Is.EqualTo(2));

            Assert.That(context.GetActiveRequests().First().RemainingNumber, Is.EqualTo(120));
            Assert.That(context.GetActiveRequests().First().Direction, Is.EqualTo(Direction.Sell));
            Assert.That(context.GetActiveRequests().First().InstrumentId, Is.EqualTo("test_id"));

            Assert.That(context.GetActiveRequests().ToList()[1].RemainingNumber, Is.EqualTo(10));
            Assert.That(context.GetActiveRequests().ToList()[1].Direction, Is.EqualTo(Direction.Buy));
            Assert.That(context.GetActiveRequests().ToList()[1].InstrumentId, Is.EqualTo("test_id"));
            Assert.That(context.GetRequestsHistory().Count, Is.EqualTo(3));
        }


        [Test]
        public void Force_close_positions_when_not_working_period_was_execute()
        {

            _balanceMock.Setup(x => x.GetTotal()).Returns(1000);
            IDictionary<string, Tick> tick = new Dictionary<string, Tick>
            {
                {
                    "test_id", new Tick.Builder()
                        .WithDate(new DateTime(2016, 9, 12, 20, 46, 0))
                        .WithId("test_id")
                        .WithPrice(160)
                        .WithVolume(80)
                        .Build()
                }
            };

            OpenPositionRequest request = new OpenPositionRequest.Builder()
                .InstrumentId("test_id")
                .Number(10)
                .Direction(Direction.Buy)
                .Build();

            IEnumerable<Transaction> transactions = new List<Transaction>
            {
                new Transaction.Builder().InstrumentId("test_id").Direction(Direction.Buy).Number(20).ExecutedPrice(123).Build(),
                new Transaction.Builder().InstrumentId("test_id").Direction(Direction.Buy).Number(40).ExecutedPrice(123).Build(),
                new Transaction.Builder().InstrumentId("test_id").Direction(Direction.Buy).Number(60).ExecutedPrice(123).Build(),
                new Transaction.Builder().InstrumentId("test_id").Direction(Direction.Sell).Number(10).ExecutedPrice(123).Build()
            };

            _transactionHolderMock.Setup(x => x.GetOpenTransactions("test_id")).Returns(transactions);

            ITransactionsContext context = new TransactionsContext(new Dictionary<string, BrokerCost>
            {
                { "test_id", new BrokerCost{ Coverage = 0.10, TransactionCost = 2}}
            });

            context.SetUpWorkingPeriod(new Dictionary<string, WorkingPeriod>
            {
                {"test_id", new WorkingPeriod{Open = new TimeSpan(0, 10, 30, 0), Close = new TimeSpan(0, 20, 0, 0)}}
            });

            context.ProcessTick(new Dictionary<string, Tick>
            {
                {"test_id", new Tick.Builder().WithDate(new DateTime(2016, 9, 12,11, 45, 0)).WithId("test_id").WithPrice(120).Build()}
            }, new DateTime(2016, 9, 12, 11, 45, 0));

            Assert.That(context.OpenPosition(request), Is.True);

            context.ProcessTick(tick, new DateTime(2016, 9, 12, 20, 46, 0));
            _balanceMock.Verify(x => x.AddTransactionCost(2, It.IsAny<DateTime>(), It.IsAny<Guid>()), Times.Exactly(1));
            Assert.That(context.GetActiveRequests().Count, Is.EqualTo(1));

            Assert.That(context.GetActiveRequests().First().RemainingNumber, Is.EqualTo(10));
            Assert.That(context.GetActiveRequests().First().Direction, Is.EqualTo(Direction.Buy));
            Assert.That(context.GetActiveRequests().First().InstrumentId, Is.EqualTo("test_id"));

        }
    }
}
