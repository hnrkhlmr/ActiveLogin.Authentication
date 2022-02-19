using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActiveLogin.Authentication.BankId.AspNetCore.Models;
using ActiveLogin.Identity.Swedish;
using Microsoft.AspNetCore.Authentication;

namespace ActiveLogin.Authentication.BankId.AspNetCore.Serialization
{
    internal class BankIdLoginOptionsSerializer : IDataSerializer<BankIdLoginOptions>
    {
        private const int FormatVersion = 4;
        private const char CertificatePoliciesSeparator = ';';

        public byte[] Serialize(BankIdLoginOptions model)
        {
            using var memory = new MemoryStream();
            using var writer = new BinaryWriter(memory);

            writer.Write(FormatVersion);

            writer.Write(string.Join(CertificatePoliciesSeparator.ToString(), model.CertificatePolicies ?? new List<string>()));
            writer.Write(model.PersonalIdentityNumber?.To12DigitString() ?? string.Empty);
            writer.Write(model.AllowChangingPersonalIdentityNumber);
            writer.Write(model.SameDevice);
            writer.Write(model.AllowBiometric);
            writer.Write(model.UseQrCode);
            writer.Write(model.CancelReturnUrl ?? string.Empty);
            writer.Write(model.StateCookieName);

            writer.Flush();
            return memory.ToArray();
        }

        public BankIdLoginOptions Deserialize(byte[] data)
        {
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);

            if (reader.ReadInt32() != FormatVersion)
            {
                throw new IncompatibleSerializationVersion(nameof(BankIdLoginOptions));
            }

            var certificatePolicies = reader.ReadString().Split(new[] { CertificatePoliciesSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var personalIdentityNumberString = reader.ReadString();
            var personalIdentityNumber = string.IsNullOrEmpty(personalIdentityNumberString) ? null : PersonalIdentityNumber.Parse(personalIdentityNumberString, StrictMode.Off);
            var allowChangingPersonalIdentityNumber = reader.ReadBoolean();
            var autoLaunch = reader.ReadBoolean();
            var allowBiometric = reader.ReadBoolean();
            var displayQrCode = reader.ReadBoolean();
            var cancelReturnUrl = reader.ReadString();
            var stateCookieName = reader.ReadString();

            return new BankIdLoginOptions(
                certificatePolicies,
                personalIdentityNumber,
                allowChangingPersonalIdentityNumber,
                autoLaunch,
                allowBiometric,
                displayQrCode,
                cancelReturnUrl,
                stateCookieName
            );
        }
    }
}
