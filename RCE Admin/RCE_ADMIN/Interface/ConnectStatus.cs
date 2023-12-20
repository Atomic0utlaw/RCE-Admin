namespace RCE_ADMIN.Interface
{
    public static class ConnectStatus
    {
        public static void SetText(string text)
        {
            Form1.Status.Caption = "Status : <b>" + text + "</b>";
        }
    }
}
