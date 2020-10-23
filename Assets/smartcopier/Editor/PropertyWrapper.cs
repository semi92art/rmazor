using UnityEditor;

namespace SmartCopier
{
	public class PropertyWrapper
	{
		private class SerializedPropertyRef
		{
			private readonly UnityEngine.Object _unityObj;
			private readonly string _propertyPath;

			public SerializedPropertyRef(UnityEngine.Object unityObj, SerializedProperty prop)
			{
				_unityObj = unityObj;
				_propertyPath = prop.propertyPath;
			}

			public SerializedProperty GetProperty()
			{
				var so = new SerializedObject(_unityObj);
				return so.FindProperty(_propertyPath);
			}
		}

		private readonly SerializedPropertyRef _propertyRef;

		public SerializedProperty SerializedProperty { get { return _propertyRef.GetProperty(); } }
		public string Name { get; private set; }
		public bool Checked { get; set; }

		public PropertyWrapper(UnityEngine.Object unityObj, SerializedProperty property)
		{
			_propertyRef = new SerializedPropertyRef(unityObj, property);
			Name = property.displayName;
			Checked = true;
		}
	}
}

