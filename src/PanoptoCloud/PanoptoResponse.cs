namespace PepperDash.Essentials.Plugins
{
    /// <summary>
    /// A reply from the Panopto API: the status code and the body, read in full.
    /// </summary>
    /// <remarks>
    /// Null in place of one of these means nothing came back at all — a timeout or a refused
    /// connection — which is a different thing from the server answering with an error.
    /// </remarks>
    public class PanoptoResponse
    {
        public PanoptoResponse(int code, string content)
        {
            Code = code;
            Content = content;
        }

        public int Code { get; private set; }

        public string Content { get; private set; }
    }
}
