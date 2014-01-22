using System;

namespace EntityFramework.Debug.DebugVisualization.ViewModels
{
    public class EntityTypeFilterViewModel : AViewModel
    {
        public string TypeName { get; private set; }

        private bool _isSelected = true;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(); _onSelectionChanged.Invoke(); }
        }

        private readonly Action _onSelectionChanged;

        public EntityTypeFilterViewModel(string entityTypeName, Action onSelectionChanged)
        {
            _onSelectionChanged = onSelectionChanged;
            TypeName = entityTypeName;
        }
    }
}