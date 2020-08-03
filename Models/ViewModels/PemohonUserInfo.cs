using System.Runtime.Serialization;

namespace PsefApi.Models
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
        public uint Id => Pemohon.Id;

        /// <summary>
        /// Gets or sets the associated user identifier.
        /// </summary>
        /// <value>The associated user identifier.</value>
        public string UserId => Pemohon.UserId;

        /// <summary>
        /// Gets or sets the Pemohon phone number.
        /// </summary>
        /// <value>The Pemohon's phone number.</value>
        public string Phone => Pemohon.Phone;

        /// <summary>
        /// Gets or sets the Pemohon address.
        /// </summary>
        /// <value>The Pemohon's address.</value>
        public string Address => Pemohon.Address;

        /// <summary>
        /// Gets or sets the Pemohon NIB.
        /// </summary>
        /// <value>The Pemohon's NIB.</value>
        public string Nib => Pemohon.Nib;

        /// <summary>
        /// Gets or sets the Pemohon apoteker name.
        /// </summary>
        /// <value>The Pemohon's apoteker name.</value>
        public string ApotekerName => Pemohon.ApotekerName;

        /// <summary>
        /// Gets or sets the Pemohon apoteker email.
        /// </summary>
        /// <value>The Pemohon's apoteker email.</value>
        public string ApotekerEmail => Pemohon.ApotekerEmail;

        /// <summary>
        /// Gets or sets the Pemohon apoteker phone number.
        /// </summary>
        /// <value>The Pemohon's apoteker phone number.</value>
        public string ApotekerPhone => Pemohon.ApotekerPhone;

        /// <summary>
        /// Gets or sets the Pemohon apoteker STRA number.
        /// </summary>
        /// <value>The Pemohon's apoteker STRA number.</value>
        public string StraNumber => Pemohon.StraNumber;

        /// <summary>
        /// Gets or sets the Pemohon apoteker STRA document url.
        /// </summary>
        /// <value>The Pemohon's apoteker STRA document url.</value>
        public string StraUrl => Pemohon.StraUrl;

        /// <summary>
        /// Gets or sets the User name.
        /// </summary>
        /// <value>The User's name.</value>
        public string Username => UserInfo.Name;

        /// <summary>
        /// Gets or sets the User email.
        /// </summary>
        /// <value>The User's email.</value>
        public string Email => UserInfo.Email;
    }
}