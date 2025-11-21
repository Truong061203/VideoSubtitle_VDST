using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDST.Commands;

namespace VDST.Model.Interfaces
{
    public interface IViewModel
    {
        void Init_Data();

    }
    public interface IWindowViewModel : IViewModel
    {
        VfxCommand CloseCommand { get; set; }
    }
}
