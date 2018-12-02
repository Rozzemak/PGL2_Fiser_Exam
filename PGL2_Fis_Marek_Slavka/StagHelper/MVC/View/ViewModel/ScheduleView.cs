using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PGL2_Fis_Marek_Slavka.Annotations;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services.AllServices;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.View.ViewModel
{
    class ScheduleContentModel
    {
        public Button ScheduleActionButton = new Button()
        {
            
        };

    }

    class ScheduleView : UIElement
    {
        public Grid ScheduleRootGrid { get; private set; }
        public StackPanel DateWeekStackPanel { get; private set; }
        public StackPanel DateDayStackPanel { get; private set; }
        public StackPanel FilterStackPanel { get; private set; }
        public StackPanel ScheduleContentStackPanel { get; private set; }
        public ScheduleContentModel ScheduleContentModel { get; private set; }


        public ScheduleView([NotNull] Grid scheduleRootGrid, [NotNull] StackPanel dateWeekStackPanel, [NotNull] StackPanel dateDayStackPanel,
            [NotNull] StackPanel filterStackPanel, [NotNull] StackPanel scheduleContentStackPanel, [NotNull] ScheduleContentModel scheduleContentModel)
        {
            this.ScheduleRootGrid = scheduleRootGrid;
            this.DateWeekStackPanel = dateWeekStackPanel;
            this.DateDayStackPanel = dateDayStackPanel;
            this.FilterStackPanel = filterStackPanel;
            this.ScheduleContentStackPanel = scheduleContentStackPanel;
            this.ScheduleContentModel = scheduleContentModel;
        }

        public void PopulateGrid()
        {

        }

        

    }
}
