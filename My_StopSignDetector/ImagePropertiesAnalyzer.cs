using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace My_StopSignDetector
{
    interface ImagePropertiesAnalyzer
    {
        int CountContours(System.Drawing.Bitmap temp);
        double getarea(PointF[] pts);
    }
}
