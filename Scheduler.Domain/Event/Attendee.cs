using Scheduler.Domain.Common;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Scheduler.Domain.Event
{
    public class Attendee
    {
        public const int NameMaxLength = 100;
        public const int EmailMaxLength = 254;

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;

        /// <summary>null = not responded yet, true = accepted, false = rejected.</summary>
        public bool? IsAttending { get; private set; }

        private Attendee() { } // for EF: materialisation only,

        internal Attendee(string name, string email)
        {
            Id = Guid.NewGuid();
            Rename(name);
            Email = NormalizeEmail(email);
        }

        internal void Rename(string name)
        {
            name = name?.Trim() ?? "";
            if (name.Length == 0) throw new DomainException("Attendee name is required.");
            if (name.Length > NameMaxLength) throw new DomainException($"Attendee name must be at most {NameMaxLength} characters.");
            Name = name;
        }

        internal void Respond(bool attending) => IsAttending = attending;

        internal static string NormalizeEmail(string email)
        {
            email = email?.Trim().ToLowerInvariant() ?? "";
            if (email.Length == 0) throw new DomainException("Attendee email is required.");
            if (email.Length > EmailMaxLength) throw new DomainException($"Attendee email must be at most {EmailMaxLength} characters.");
            if (!MailAddress.TryCreate(email, out var parsed) || parsed.Address != email)
                throw new DomainException($"'{email}' is not a valid email address.");
            return email;
        }

    }
}
