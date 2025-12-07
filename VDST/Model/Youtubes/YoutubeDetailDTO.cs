using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VDST.Model.Youtubes
{
    public class YoutubeDetailDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public int Duration { get; set; } = 0;

        [JsonIgnore]
        public SubtitleDTO[] OriginSubs { get; set; } = Array.Empty<SubtitleDTO>();
        [JsonIgnore]
        public SubtitleDTO[] AutoSubs { get; set; } = Array.Empty<SubtitleDTO>();
    }

    public class SubtitleDTO
    {
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        public string Ext { get; set; }
        public string Url { get; set; }
    }
}
