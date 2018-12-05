namespace Models.Shared
{
    public class UserModel
    {
        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    return string.Format("{0}/{1}", ID, Name);
                }
                else
                {
                    return ID;
                }
            }
        }

        public string Email { get; set; }

        public string ManagerID { get; set; }

        public override bool Equals(object Object)
        {
            var key = Object as UserModel;

            return Equals(key);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public bool Equals(UserModel User)
        {
            if (User == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(User.ID))
            {
                return false;
            }

            return this.ID.Equals(User.ID);
        }
    }
}
