using System.Runtime.Serialization;

namespace PsefApiOData.Models
{
    /// <summary>
    /// Represents a Pemohon with User information.
    /// </summary>
    public class PemohonUserInfo
    {
        /// <summary>
        /// Gets or sets the Pemohon.
        /// </summary>
        /// <value>The Pemohon.</value>
        [IgnoreDataMember]
        public Pemohon Pemohon { get; set; }

        /// <summary>
        /// Gets or sets the User.
        /// </summary>
        /// <value>The User.</value>
        [IgnoreDataMember]
        public UserInfo UserInfo { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the Pemohon.
        /// </summary>
        /// <value>The Pemohon's unique identifier.</value>
        public uint Id
        {
            get => Pemohon.Id;
            set => Pemohon.Id = value;
        }

        /// <summary>
        /// Gets or sets the associated user identifier.
        /// </summary>
        /// <value>The associated user identifier.</value>
        public string UserId
        {
            get => Pemohon.UserId;
            set => Pemohon.UserId = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon phone number.
        /// </summary>
        /// <value>The Pemohon's phone number.</value>
        public string Phone
        {
            get => Pemohon.Phone;
            set => Pemohon.Phone = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon address.
        /// </summary>
        /// <value>The Pemohon's address.</value>
        public string Address
        {
            get => Pemohon.Address;
            set => Pemohon.Address = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon NIB.
        /// </summary>
        /// <value>The Pemohon's NIB.</value>
        public string Nib
        {
            get => Pemohon.Nib;
            set => Pemohon.Nib = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon apoteker name.
        /// </summary>
        /// <value>The Pemohon's apoteker name.</value>
        public string ApotekerName
        {
            get => Pemohon.ApotekerName;
            set => Pemohon.ApotekerName = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon apoteker email.
        /// </summary>
        /// <value>The Pemohon's apoteker email.</value>
        public string ApotekerEmail
        {
            get => Pemohon.ApotekerEmail;
            set => Pemohon.ApotekerEmail = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon apoteker phone number.
        /// </summary>
        /// <value>The Pemohon's apoteker phone number.</value>
        public string ApotekerPhone
        {
            get => Pemohon.ApotekerPhone;
            set => Pemohon.ApotekerPhone = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon apoteker STRA number.
        /// </summary>
        /// <value>The Pemohon's apoteker STRA number.</value>
        public string StraNumber
        {
            get => Pemohon.StraNumber;
            set => Pemohon.StraNumber = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon apoteker STRA document url.
        /// </summary>
        /// <value>The Pemohon's apoteker STRA document url.</value>
        public string StraUrl
        {
            get => Pemohon.StraUrl;
            set => Pemohon.StraUrl = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon name.
        /// </summary>
        /// <value>The Pemohon's name.</value>
        public string Name
        {
            get => UserInfo.Name;
            set => UserInfo.Name = value;
        }

        /// <summary>
        /// Gets or sets the Pemohon email.
        /// </summary>
        /// <value>The Pemohon's email.</value>
        public string Email
        {
            get => UserInfo.Email;
            set => UserInfo.Email = value;
        }
    }
}