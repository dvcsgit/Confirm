using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.PipelinePatrol.FileManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "FilePath", ResourceType = typeof(Resources.Resource))]
        public string FullPathDescription { get; set; }

        public int Size { get; set; }

        [Display(Name = "FileSize", ResourceType = typeof(Resources.Resource))]
        public string FileSize
        {
            get
            {
                var index = 0;

                var size = Size;

                while (size > 1024)
                {
                    size = size / 1024;
                    index++;
                }

                return string.Format("{0} {1}", size, Define.FileSizeDescription[index]);
            }
        }

        public FormInput FormInput { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
