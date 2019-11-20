using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace InfoLocker
{
    public class VirtualizedWrapPanel : WrapPanel
    {
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            return base.MeasureOverride(constraint);
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
    }
}
