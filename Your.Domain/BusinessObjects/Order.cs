﻿namespace Your.Domain.BusinessObjects
{
    public class Order
    {
        public long Id { get; set; }
        public string OrderId { get; set; }
        public long ItemId { get; set; }
        public int Count { get; set; }
        public long Version { get; set; }
    }
}
