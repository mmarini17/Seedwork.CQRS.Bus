using System;
using System.Threading.Tasks;
using FluentAssertions;
using Seedwork.CQRS.Bus.Core;
using Seedwork.CQRS.Bus.IntegrationTests.Stubs;
using Seedwork.CQRS.Bus.IntegrationTests.Utils;
using Xunit;

namespace Seedwork.CQRS.Bus.IntegrationTests
{
    public class BusObserverTests : IClassFixture<RabbitMQUtils>, IClassFixture<RabbitMQConnectionFactory>
    {
        public BusObserverTests(RabbitMQUtils rabbitMqUtils, RabbitMQConnectionFactory factory)
        {
            _rabbitMqUtils = rabbitMqUtils ?? throw new ArgumentNullException(nameof(rabbitMqUtils));
            _connection = factory?.Create() ?? throw new ArgumentNullException(nameof(factory));
        }

        private readonly RabbitMQUtils _rabbitMqUtils;
        private readonly IBusConnection _connection;

        [Fact]
        public async Task Given_observer_should_notify()
        {
            var exchange = StubExchange.Instance;
            var queue = new StubQueue(nameof(Given_observer_should_notify), nameof(Given_observer_should_notify));
            _rabbitMqUtils.Purge(exchange, queue);

            var observer = new StubObserver(queue);
            await _connection.Subscribe(observer);

            _rabbitMqUtils.Publish(exchange, queue.RoutingKey,
                new StubNotification(nameof(BusObserverTests)));

            _rabbitMqUtils.Flush();

            observer.Value.Should().NotBeNull();
            observer.Value.Message.Should().Be(nameof(BusObserverTests));
        }

        [Fact]
        public async Task Given_observer_when_dispose_should_complete()
        {
            var observer = new StubObserver();

            await _connection.Subscribe(observer);

            observer.Dispose();
            observer.Completed.Should().BeTrue();
        }

        [Fact]
        public async Task Given_observer_when_dispose_twice_should_not_throw()
        {
            var observer = new StubObserver();

            await _connection.Subscribe(observer);

            Action action = () =>
            {
                observer.Dispose();
                observer.Dispose();
            };
            action.Should().NotThrow();
        }

        [Fact]
        public async Task Given_observer_when_success_should_ack()
        {
            var exchange = StubExchange.Instance;
            var queue = new StubQueue(nameof(Given_observer_when_success_should_ack),
                nameof(Given_observer_when_success_should_ack));
            _rabbitMqUtils.Purge(exchange, queue);

            var observer = new StubObserver(queue);
            await _connection.Subscribe(observer);

            _rabbitMqUtils.Publish(exchange, queue.RoutingKey,
                new StubNotification(nameof(BusObserverTests)));

            _rabbitMqUtils.Flush();

            observer.Value.Should().NotBeNull();
            observer.Value.Message.Should().Be(nameof(BusObserverTests));

            _rabbitMqUtils.MessageCount(exchange, queue).Should().Be(0);
        }

        [Fact]
        public async Task Given_observer_when_throw_should_notify_error()
        {
            var exchange = StubExchange.Instance;
            var queue = new StubQueue(nameof(Given_observer_when_throw_should_notify_error),
                nameof(Given_observer_when_throw_should_notify_error));
            _rabbitMqUtils.Purge(exchange, queue);

            var observer = new StubObserver(queue);
            await _connection.Subscribe(observer);

            _rabbitMqUtils.Publish(exchange, queue.RoutingKey, 10);

            _rabbitMqUtils.Flush();

            observer.Error.Should().NotBeNull();
        }

        [Fact]
        public async Task Given_observer_when_throw_should_requeue()
        {
            var exchange = StubExchange.Instance;
            var queue = new StubQueue(nameof(Given_observer_when_throw_should_requeue),
                nameof(Given_observer_when_throw_should_requeue));
            _rabbitMqUtils.Purge(exchange, queue);

            var observer = new StubObserver(queue);
            await _connection.Subscribe(observer);

            _rabbitMqUtils.Publish(exchange, queue.RoutingKey, 10);

            _rabbitMqUtils.Flush();

            observer.Error.Should().NotBeNull();
            observer.Dispose();

            observer = new StubObserver(queue);
            await _connection.Subscribe(observer);

            _rabbitMqUtils.Flush();

            observer.Error.Should().NotBeNull();
        }
    }
}