using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace FaceRoll.Common
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName]String info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        protected void SetValue<T>(Expression<Func<T>> expression, T value)
        {
            MemberExpression body = (MemberExpression)expression.Body;
            string propName = body.Member.Name;

            var ce = (ConstantExpression)body.Expression;
            object model = ce.Value;

            model.GetType().GetProperty(propName).SetValue(model, value);
            NotifyPropertyChanged(propName);
        }
    }
}