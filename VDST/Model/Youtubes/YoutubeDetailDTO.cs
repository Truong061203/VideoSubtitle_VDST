using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDST.Model.Youtubes
{
    public class YoutubeDetailDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;

        public int Duration { get; set; } = 0;
    }
}
