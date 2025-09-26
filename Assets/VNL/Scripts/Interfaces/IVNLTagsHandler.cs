using System.Collections.Generic;

public interface IVNLTagsHandler
{
    bool IsVNLTag(string Tag);
    bool IsVNLStyleTag(string Tag);
    (string TagName, bool IsOpener, object TagParameter) GetVNLTagInfo(string VNLTag);
    (string StyleTagName, bool IsOpener, List<object> TagParameters) GetVNLStyleTagInfo(string VNLStyleTag);
}
