using System.Collections.Generic;
using System.Collections.ObjectModel;
using RabbitMQ.Client;

namespace Seedwork.CQRS.Bus.Core
{
    public class Exchange
    {
        private IDictionary<string, object> _arguments = new Dictionary<string, object>();

        private Exchange(ExchangeName name, ExchangeType type)
        {
            Name = name;
            Type = type;
            Durability = Durability.Transient;
            IsInternal = false;
            IsAutoDelete = false;
        }

        public ExchangeName Name { get; private set; }
        public ExchangeType Type { get; private set; }
        public Durability Durability { get; private set; }
        public bool IsAutoDelete { get; private set; }
        public bool IsInternal { get; set; }
        public IReadOnlyDictionary<string, object> Arguments => new ReadOnlyDictionary<string, object>(_arguments);

        public static Exchange Create(string name, ExchangeType type)
        {
            return new Exchange(ExchangeName.Create(name), type);
        }

        /// <summary>
        /// The exchange will delete itself after at least one queue or exchange has been bound to this one, and then all queues or exchanges have been unbound.  
        /// </summary>
        /// <returns></returns>
        public Exchange WithAutoDelete()
        {
            IsAutoDelete = true;

            return this;
        }

        public Exchange WithoutAutoDelete()
        {
            IsAutoDelete = false;

            return this;
        }

        /// <summary>
        /// Clients cannot publish to this exchange directly. It can only be used with exchange to exchange bindings. 
        /// </summary>
        /// <returns></returns>
        public Exchange AsInternal()
        {
            IsInternal = true;

            return this;
        }

        /// <summary>
        /// Clients can publish to this exchange directly. 
        /// </summary>
        /// <returns></returns>
        public Exchange AsPublic()
        {
            IsInternal = false;

            return this;
        }

        /// <summary>
        /// If messages to this exchange cannot otherwise be routed, send them to the alternate exchange named here.
        /// </summary>
        /// <param name="exchangeName">Exchange name.</param>
        /// <returns></returns>
        public Exchange WithAlternateExchange(ExchangeName exchangeName)
        {
            const string key = "alternate-exchange";

            _arguments.Remove(key);
            _arguments.Add(key, exchangeName.Value);

            return this;
        }

        internal void Declare(IModel channel)
        {
            channel.ExchangeDeclare(Name.Value, Type.Value, Durability.IsDurable, IsAutoDelete, _arguments);
        }
    }
}