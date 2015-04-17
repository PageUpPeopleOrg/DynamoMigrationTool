using System;

namespace PageUp.Formsmith.Database.MigrationFramework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MigrationAttribute : Attribute
    {
        public MigrationProfile Profile { get; private set; }
        public int Order { get; private set; }

        public MigrationAttribute(MigrationProfile profile, int order)
        {
            Profile = profile;
            Order = order;
        }
    }
}