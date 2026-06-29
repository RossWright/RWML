using RossWright.MetalInjection;
using RossWright.MetalNexus.Testbed.Shared;

namespace RossWright.MetalNexus.Testbed.Server;

/// <summary>
/// Thread-safe in-memory store for customers, notes, and uploaded file data.
/// Seeded with 5 sample customers on construction.
/// </summary>
[Singleton(typeof(InMemoryRepository))]
internal class InMemoryRepository
{
    private readonly Lock _lock = new();
    private int _nextCustomerId = 6;
    private int _nextNoteId = 1;
    private int _nextDocumentId = 1;

    private readonly List<Customer> _customers = [
        new() { Id = 1, Name = "Alice Nakamura",   Email = "alice@example.com",   Phone = "555-0101", CreatedAt = DateTime.UtcNow.AddDays(-30) },
        new() { Id = 2, Name = "Bob Okonkwo",      Email = "bob@example.com",     Phone = "555-0102", CreatedAt = DateTime.UtcNow.AddDays(-20) },
        new() { Id = 3, Name = "Carmen Reyes",     Email = "carmen@example.com",  Phone = "555-0103", CreatedAt = DateTime.UtcNow.AddDays(-15) },
        new() { Id = 4, Name = "David Lindqvist",  Email = "david@example.com",   Phone = "555-0104", CreatedAt = DateTime.UtcNow.AddDays(-10) },
        new() { Id = 5, Name = "Evelyn Chukwu",    Email = "evelyn@example.com",  Phone = "555-0105", CreatedAt = DateTime.UtcNow.AddDays(-5)  },
    ];

    // ── Customers ────────────────────────────────────────────────────────────

    public List<Customer> GetAllCustomers()
    {
        lock (_lock)
            return _customers.Select(Clone).ToList();
    }

    public Customer? GetCustomer(int id)
    {
        lock (_lock)
            return _customers.FirstOrDefault(c => c.Id == id) is { } c ? Clone(c) : null;
    }

    public Customer CreateCustomer(string name, string email, string phone)
    {
        lock (_lock)
        {
            var customer = new Customer
            {
                Id = _nextCustomerId++,
                Name = name,
                Email = email,
                Phone = phone,
                CreatedAt = DateTime.UtcNow
            };
            _customers.Add(customer);
            return Clone(customer);
        }
    }

    public Customer? UpdateCustomer(int id, string name, string email, string phone)
    {
        lock (_lock)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer is null) return null;
            customer.Name = name;
            customer.Email = email;
            customer.Phone = phone;
            return Clone(customer);
        }
    }

    public bool DeleteCustomer(int id)
    {
        lock (_lock)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer is null) return false;
            _customers.Remove(customer);
            return true;
        }
    }

    public bool EmailExists(string email, int? excludeId = null)
    {
        lock (_lock)
            return _customers.Any(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                                       && c.Id != (excludeId ?? -1));
    }

    // ── Notes ────────────────────────────────────────────────────────────────

    public CustomerNote? AddNote(int customerId, string text)
    {
        lock (_lock)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == customerId);
            if (customer is null) return null;
            var note = new CustomerNote
            {
                Id = _nextNoteId++,
                CustomerId = customerId,
                Text = text,
                CreatedAt = DateTime.UtcNow
            };
            customer.Notes.Add(note);
            return note;
        }
    }

    // ── Documents ────────────────────────────────────────────────────────────

    public CustomerDocument? AddDocument(int customerId, string fileName, string contentType, byte[] data)
    {
        lock (_lock)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == customerId);
            if (customer is null) return null;
            var doc = new CustomerDocument
            {
                Id = _nextDocumentId++,
                CustomerId = customerId,
                FileName = fileName,
                ContentType = contentType,
                Data = data,
                UploadedAt = DateTime.UtcNow
            };
            customer.Documents.Add(doc);
            return doc;
        }
    }

    public CustomerDocument? GetDocument(int customerId, int documentId)
    {
        lock (_lock)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == customerId);
            return customer?.Documents.FirstOrDefault(d => d.Id == documentId);
        }
    }

    public bool SetAvatarUrl(int customerId, string? avatarUrl)
    {
        lock (_lock)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == customerId);
            if (customer is null) return false;
            customer.AvatarUrl = avatarUrl;
            return true;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Customer Clone(Customer c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Email = c.Email,
        Phone = c.Phone,
        CreatedAt = c.CreatedAt,
        AvatarUrl = c.AvatarUrl,
        Notes = [.. c.Notes],
        Documents = c.Documents.Select(d => new CustomerDocument
        {
            Id = d.Id,
            CustomerId = d.CustomerId,
            FileName = d.FileName,
            ContentType = d.ContentType,
            Data = d.Data,
            UploadedAt = d.UploadedAt
        }).ToList()
    };
}

// ── Domain entities (server-side only) ───────────────────────────────────────

internal class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AvatarUrl { get; set; }
    public List<CustomerNote> Notes { get; set; } = [];
    public List<CustomerDocument> Documents { get; set; } = [];
}

internal class CustomerNote
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

internal class CustomerDocument
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public DateTime UploadedAt { get; set; }
}
