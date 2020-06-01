namespace ProtonVPN.Core.User
{
    public class UserLocationEventArgs
    {
        public UserLocationEventArgs(UserLocationState state, UserLocation location)
        {
            State = state;
            Location = location;
        }

        public UserLocationState State { get; }

        public UserLocation Location { get; }
    }
}
