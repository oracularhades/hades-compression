using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadesCompression
{
    public class Objects
    {
        public class ffprobevideoinfo
        {
            public Objects.ffprobevideoinfostream[] streams { get; set; }
            public Objects.ffprobevideoinfoformat format { get; set; }
        }
        public class ffprobevideoinfostream
        {
            public int index { get; set; }
            public string codec_name { get; set; }
            public string codec_long_name { get; set; }
            public string profile { get; set; }
            public string codec_type { get; set; }
            public string codec_tag { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int coded_width { get; set; }
            public int coded_height { get; set; }
            public int closed_captions { get; set; }
            public int has_b_frames { get; set; }
            public string sample_aspect_ration { get; set; }
            public string display_aspect_ratio { get; set; }
            public string pix_fmt { get; set; }
            public int level { get; set; }
            public string color_range { get; set; }
            public string color_space { get; set; }
            public string color_transfer { get; set; }
            public string color_primaries { get; set; }
            public string chroma_location { get; set; }
            public string field_order { get; set; }
            public int refs { get; set; }
            public string is_avc { get; set; }
            public string nal_length_size { get; set; }
            public string r_frame_rate { get; set; }
            public string avg_frame_rate { get; set; }
            public string time_base { get; set; }
            public int start_pts { get; set; }
            public string start_time { get; set; }
            public string bits_per_raw_sample { get; set; }
            public Objects.ffprobevideoinfostream_disposition disposition { get; set; }
            public Objects.ffprobevideoinfostream_tags tags { get; set; }
        }
        public class ffprobevideoinfostream_disposition
        {
            public int dub { get; set; }
            public int original { get; set; }
            public int comment { get; set; }
            public int lyrics { get; set; }
            public int karaoke { get; set; }
            public int forced { get; set; }
            public int hearing_impaired { get; set; }
            public int visual_impaired { get; set; }
            public int clean_effects { get; set; }
            public int attached_pic { get; set; }
            public int timed_thumbnails { get; set; }
        }
        public class ffprobevideoinfostream_tags
        {
            public string DURATION { get; set; }
        }
        public class ffprobevideoinfoformat
        {
            public string filename { get; set; }
            public int nb_streams { get; set; }
            public int nb_programs { get; set; }
            public string format_name { get; set; }
            public string format_long_name { get; set; }
            public string start_time { get; set; }
            public string duration { get; set; }
            public string size { get; set; }
            public string bit_rate { get; set; }
            public int probe_score { get; set; }
            public Objects.ffprobevideoinfotags tags { get; set; }
        }
        public class ffprobevideoinfotags
        {
            public string ENCODER { get; set; }
        }
        public class encoding_config
        {
            public string input_file { get; set; }
            public string output_file { get; set; }
            public string output_format { get; set; }
            public int quality { get; set; }
            public string vcodec { get; set; }
            public string max_rate { get; set; }
            public int crf { get; set; }
            public string preset { get; set; }
            public List<int> audio_tracks { get; set; }
        }
        public class get_hadescompression_directories {
            public string input_directory { get; set; }
            public string output_directory { get; set; }
            public string input_directory_processing { get; set; }
        }
        public class queue_item {
            public bool paused { get; set; }
            public bool compressing { get; set; }
            public string status { get; set; }
            public string path { get; set; }
            public string encoded { get; set; }
            public string max_encoded { get; set; }
        }
        public class obs_output {
            public obs_stream stream { get; set; }
            public obs_recording recording { get; set; }
        }
        public class obs_stream {
            public bool outputActive { get; set; }
            public bool outputReconnecting { get; set; }
        }
         public class obs_recording {
            public bool outputActive { get; set; }
            public bool outputPaused { get; set; }
        }
    }
}
