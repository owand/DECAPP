using DECAPP.Controls;
using DECAPP.iOS.Controls;
using System.Security;
using UIKit;
using Xamarin.Forms.Platform.iOS;

[assembly: Xamarin.Forms.ExportRenderer(typeof(CustomViewCell), typeof(CustomViewCell_iOS))]

namespace DECAPP.iOS.Controls
{
    public class CustomViewCell_iOS : ViewCellRenderer
    {
        public CustomViewCell_iOS()
        {
        }

        #region вариант 1

        //public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        //{
        //    var cell = base.GetCell(item, reusableCell, tv);
        //    cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
        //    return cell;
        //}

        #endregion вариант 1

        #region вариант 2 (не требует создания "общего" класса, но ограничен выбор цвета)

        //public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        //{
        //    var cell = base.GetCell(item, reusableCell, tv);

        //    cell.SelectedBackgroundView = new UIView
        //    {
        //        BackgroundColor = UIColor.DarkGray,
        //    };

        //    return cell;
        //}

        #endregion вариант 2 (не требует создания "общего" класса, но ограничен выбор цвета)

        [SecurityCritical]
        public override UITableViewCell GetCell(Xamarin.Forms.Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            UITableViewCell cell = base.GetCell(item, reusableCell, tv);
            CustomViewCell view = item as CustomViewCell;
            cell.SelectedBackgroundView = new UIView
            {
                BackgroundColor = view.SelectedBackgroundColor.ToUIColor(),
            };
            return cell;
        }
    }
}