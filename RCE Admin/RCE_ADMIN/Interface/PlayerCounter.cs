namespace RCE_ADMIN.Interface
{
    public static class PlayerCounter
    {
        public static void SetText(int playerCount)
        {
            Form1.Counter.Caption = "Players : <b>" + playerCount.ToString() + "</b>";
        }
        public static void Reset()
        {
            SetText(0);
        }
    }
}
