using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 


namespace ABCToolkit {

#if UNITY_EDITOR
    using UnityEditor;

    /// <summary>
    /// Custom property drawer to allow the user to drag the normal data type into the editor fields as normal but also sets 
    /// additional data like the name of the object value dragged into the field which allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [CustomPropertyDrawer(typeof(ABC_ShaderReference))]
    [CustomPropertyDrawer(typeof(ABC_SliderReference))]
    [CustomPropertyDrawer(typeof(ABC_TextReference))]
    [CustomPropertyDrawer(typeof(ABC_TextureReference))]
    [CustomPropertyDrawer(typeof(ABC_MaterialReference))]
    [CustomPropertyDrawer(typeof(ABC_RawImageReference))]
    [CustomPropertyDrawer(typeof(ABC_ImageReference))]
    [CustomPropertyDrawer(typeof(ABC_Texture2DReference))]
    [CustomPropertyDrawer(typeof(ABC_CameraReference))]
    [CustomPropertyDrawer(typeof(ABC_ObjectReference))]
    [CustomPropertyDrawer(typeof(ABC_GameObjectReference))]
    [CustomPropertyDrawer(typeof(ABC_AnimationClipReference))]
    [CustomPropertyDrawer(typeof(ABC_AvatarMaskReference))]
    [CustomPropertyDrawer(typeof(ABC_AudioClipReference))]
    public class ABC_CustomReferenceDrawer : PropertyDrawer {
        SerializedProperty refValProp;
        SerializedProperty refNameProp;
        SerializedProperty refUpdateDateTimeProp;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            refNameProp = property.FindPropertyRelative("refName");
            refValProp = property.FindPropertyRelative("refVal");
            refUpdateDateTimeProp = property.FindPropertyRelative("refUpdateDateTime");

            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;




            switch (property.type) {
                case "ABC_GameObjectReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(GameObject), true);
                    break;
                case "ABC_Texture2DReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Texture2D), true);
                    break;
                case "ABC_ShaderReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Shader), true);
                    break;
                case "ABC_SliderReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Slider), true);
                    break;
                case "ABC_ObjectReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Object), true);
                    break;
                case "ABC_TextReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Graphic), true);
                    break;
                case "ABC_CameraReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Camera), true);
                    break;
                case "ABC_ImageReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Image), true);
                    break;
                case "ABC_RawImageReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(RawImage), true);
                    break;
                case "ABC_TextureReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Texture), true);
                    break;
                case "ABC_MaterialReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Material), true);
                    break;
                case "ABC_AnimationClipReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(AnimationClip), true);
                    break;
                case "ABC_AvatarMaskReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(AvatarMask), true);
                    break;
                case "ABC_AudioClipReference":
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(AudioClip), true);
                    break;
                default:
                    refValProp.objectReferenceValue = EditorGUI.ObjectField(position, label, refValProp.objectReferenceValue, typeof(Object), true);
                    break;
            }




            //Retrieve Data
            if (refValProp.objectReferenceValue != null && refNameProp.stringValue == "" || refValProp.objectReferenceValue != null && refNameProp.stringValue != ((Object)refValProp.objectReferenceValue).name) {
                Object objectSet = refValProp.objectReferenceValue as Object;

                //Set name 
                refNameProp.stringValue = objectSet.name;

                //Update time the reference was last updated
                refUpdateDateTimeProp.stringValue = System.DateTime.UtcNow.ToString();
            }


            //If data was cleared then clear the string value also 
            if (refValProp.objectReferenceValue == null && refNameProp.stringValue != "") {

                //Blank name
                refNameProp.stringValue = "";

                //Update time the reference was last updated
                refUpdateDateTimeProp.stringValue = System.DateTime.UtcNow.ToString();
            }




            EditorGUI.EndProperty();
        }

    }


#endif


    /// <summary>
    /// A GameObject datatype override, acts just like GameObject but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_GameObjectReference {

        public GameObject refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public GameObject GameObject {
            //If null then get will retrieve the object in game by name recorded
            get {

                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;


                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<GameObject> allGameObjects = new List<GameObject>();

                    foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
                        allGameObjects.Add(go);
                    }

                    //First try and find the object in scene
                    refVal = allGameObjects.Select(item => item.gameObject).Where(obj => obj.scene.name != null && obj.name == refName).FirstOrDefault();



                    //Next try and find from all files
                    if (refVal == null)
                        refVal = allGameObjects.Select(item => item.gameObject).Where(obj => obj.name == refName).FirstOrDefault();
                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }


    }


    /// <summary>
    /// A Object datatype override, acts just like Object but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_ObjectReference {

        public Object refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public Object Object {
            //If null then get will retrieve the object in game by name recorded
            get {

                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<Object> allObjects = new List<Object>();

                    foreach (Object go in Resources.FindObjectsOfTypeAll(typeof(Object)) as Object[]) {
                        allObjects.Add(go);
                    }

                    refVal = allObjects.Select(item => item).Where(obj => obj.name == refName).FirstOrDefault();
                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }

    }


    /// <summary>
    /// A Camera datatype override, acts just like Camera but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_CameraReference {

        public Camera refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public Camera Camera {
            //If null then get will retrieve the object in game by name recorded
            get {


                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<Camera> allCameraObjects = new List<Camera>();

                    foreach (Camera go in Resources.FindObjectsOfTypeAll(typeof(Camera)) as Camera[]) {

                        allCameraObjects.Add(go);
                    }

                    //try and find the object in scene
                    refVal = allCameraObjects.Select(item => item).Where(obj => obj.scene.name != null && obj.name == refName).FirstOrDefault();

                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }
    }

    /// <summary>
    /// A Slider datatype override, acts just like Slider but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_SliderReference {
        public Slider refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;


        public Slider Slider {
            //If null then get will retrieve the object in game by name recorded
            get {


                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<Slider> allSliderObjects = new List<Slider>();

                    foreach (Slider go in Resources.FindObjectsOfTypeAll(typeof(Slider)) as Slider[]) {

                        allSliderObjects.Add(go);
                    }

                    //First try and find the object in scene
                    refVal = allSliderObjects.Select(item => item).Where(obj => obj.gameObject.scene.name != null && obj.name == refName).FirstOrDefault();

                    //Next try and find from all files
                    if (refVal == null)
                        refVal = allSliderObjects.Select(item => item).Where(obj => obj.name == refName).FirstOrDefault();
                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }

    }

    /// <summary>
    /// A Text datatype override, acts just like Text but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_TextReference {
        public Graphic refVal;
        private Text refValText;
        private TMP_Text refValTMP;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public Graphic Graphic {

            //If null then get will retrieve the object in game by name recorded
            get {

                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<Graphic> allTextObjects = new List<Graphic>();

                    foreach (Graphic go in Resources.FindObjectsOfTypeAll(typeof(Graphic)) as Graphic[]) {

                        allTextObjects.Add(go);
                    }

                    //First try and find the object in scene
                    refVal = allTextObjects.Select(item => item).Where(obj => obj.gameObject.scene.name != null && obj.name == refName).FirstOrDefault();

                    //Next try and find from all files
                    if (refVal == null)
                        refVal = allTextObjects.Select(item => item).Where(obj => obj.name == refName).FirstOrDefault();
                }

                //Make sure we have atleast one component
                if (refValText == null && refValTMP == null && refVal != null) {
                    refValText = refVal.gameObject.GetComponent<Text>();
                    refValTMP = refVal.gameObject.GetComponent<TMP_Text>();
                }

                return refVal;
            }

            set {
                refVal = value;
                refValText = value.gameObject.GetComponent<Text>();
                refValTMP = value.gameObject.GetComponent<TMP_Text>();
                refName = value.name;
            }
        }


        public string text {
            get {

                if (this.Graphic != null) {
                    //Unused code but will call the graphic getter to make sure references are correct
                }

                if (refValTMP != null)
                    return refValTMP.text;

                if (refValText != null)
                    return refValText.text;

                return null; 

            }

            set {

                if (refValTMP != null)
                    refValTMP.text = value;

                if (refValText != null)
                    refValText.text = value;

            }
        }


        public Color color {
            get {

                
                if (this.Graphic != null) {
                    //Unused code but will call the graphic getter to make sure references are correct
                }

                if (refValTMP != null)
                    return refValTMP.color;

                if (refValText != null)
                    return refValText.color;

                return new Color(0, 0, 0);

            }
             
            set {

                if (refValTMP != null)
                    refValTMP.color = value;

                if (refValText != null)
                    refValText.color = value;

            }
        }


        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }


    }

    /// <summary>
    /// A Texture override, acts just like Texture but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_TextureReference {
        public Texture refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public Texture Texture {
            //If null then get will retrieve the object in game by name recorded
            get {

                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<Texture> allTextures = new List<Texture>();

                    //Might need to use Resources.LoadAll("", typeof(Texture2D))
                    foreach (Texture go in Resources.FindObjectsOfTypeAll(typeof(Texture)) as Texture[]) {

                        allTextures.Add(go);
                    }

                    refVal = allTextures.Select(item => item).Where(obj => obj.name == refName).FirstOrDefault();
                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }


    }

    /// <summary>
    /// A Material datatype override, acts just like Material but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_MaterialReference {
        public Material refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public Material Material {
            //If null then get will retrieve the object in game by name recorded
            get {


                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<Material> allMaterials = new List<Material>();

                    //Might need to use Resources.LoadAll("", typeof(Shader))
                    foreach (Material go in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[]) {
                        allMaterials.Add(go);
                    }

                    refVal = allMaterials.Select(item => item).Where(obj => obj.name == refName).FirstOrDefault();
                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }

    }

    /// <summary>
    /// A RawImage override, acts just like RawImage but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_RawImageReference {
        public RawImage refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public RawImage RawImage {
            //If null then get will retrieve the object in game by name recorded
            get {

                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<RawImage> allRawImages = new List<RawImage>();

                    //Might need to use Resources.LoadAll("", typeof(Texture2D))
                    foreach (RawImage go in Resources.FindObjectsOfTypeAll(typeof(RawImage)) as RawImage[]) {

                        allRawImages.Add(go);
                    }

                    //First try and find the object in scene
                    refVal = allRawImages.Select(item => item).Where(obj => obj.transform.gameObject.scene.name != null && obj.name == refName).FirstOrDefault();


                    //Next try and find from all files
                    if (refVal == null)
                        refVal = allRawImages.Select(item => item).Where(obj => obj.name == refName).FirstOrDefault();

                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }
    }


    /// <summary>
    /// A Image override, acts just like Image but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_ImageReference {
        public Image refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public Image Image {
            //If null then get will retrieve the object in game by name recorded
            get {

                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<Image> allImages = new List<Image>();

                    //Might need to use Resources.LoadAll("", typeof(Texture2D))
                    foreach (Image go in Resources.FindObjectsOfTypeAll(typeof(Image)) as Image[]) {

                        allImages.Add(go);
                    }

                    //First try and find the object in scene
                    refVal = allImages.Select(item => item).Where(obj => obj.transform.gameObject.scene.name != null && obj.name == refName).FirstOrDefault();


                    //Next try and find from all files
                    if (refVal == null)
                        refVal = allImages.Select(item => item).Where(obj => obj.name == refName).FirstOrDefault();

                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }

    }


    /// <summary>
    /// A Texture2D override, acts just like Texture2D but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_Texture2DReference {
        public Texture2D refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public Texture2D Texture2D {
            //If null then get will retrieve the object in game by name recorded
            get {

                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<Texture2D> allTextures = new List<Texture2D>();

                    //Might need to use Resources.LoadAll("", typeof(Texture2D))
                    foreach (Texture2D go in Resources.FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[]) {

                        allTextures.Add(go);
                    }

                    refVal = allTextures.Select(item => item).Where(obj => obj.name == refName).FirstOrDefault();
                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }

    }

    /// <summary>
    /// A Shader datatype override, acts just like Shader but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_ShaderReference {
        public Shader refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public Shader Shader {
            //If null then get will retrieve the object in game by name recorded
            get {


                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<Shader> allShaders = new List<Shader>();

                    //Might need to use Resources.LoadAll("", typeof(Shader))
                    foreach (Shader go in Resources.FindObjectsOfTypeAll(typeof(Shader)) as Shader[]) {
                        allShaders.Add(go);
                    }

                    refVal = allShaders.Select(item => item).Where(obj => obj.name == refName).FirstOrDefault();
                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }
    }


    /// <summary>
    /// A AnimationClip datatype override, acts just like an Animation Clip but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_AnimationClipReference {

        public AnimationClip refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public AnimationClip AnimationClip {
            //If null then get will retrieve the object in game by name recorded
            get {


                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<AnimationClip> allAnimationClipObjects = new List<AnimationClip>();

                    foreach (AnimationClip go in Resources.FindObjectsOfTypeAll(typeof(AnimationClip)) as AnimationClip[]) {

                        allAnimationClipObjects.Add(go);
                    }

                    //try and find the object 
                    refVal = allAnimationClipObjects.Select(item => item).Where(obj => obj.name != null && obj.name == refName).FirstOrDefault();

                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }

    }


    /// <summary>
    /// A Avatar Mask datatype override, acts just like an Avatar Mask but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_AvatarMaskReference {

        public AvatarMask refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public AvatarMask AvatarMask {
            //If null then get will retrieve the object in game by name recorded
            get {


                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<AvatarMask> allAnimationClipObjects = new List<AvatarMask>();

                    foreach (AvatarMask go in Resources.FindObjectsOfTypeAll(typeof(AvatarMask)) as AvatarMask[]) {

                        allAnimationClipObjects.Add(go);
                    }

                    //try and find the object in scene
                    refVal = allAnimationClipObjects.Select(item => item).Where(obj => obj.name != null && obj.name == refName).FirstOrDefault();

                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }

    }



    /// <summary>
    /// A Audio Clip datatype override, acts just like an Audio Clip but has additional information regarding the data type like the name of the object value
    /// allows for ABC to find the right values for functionality like saving and loading
    /// </summary>
    [System.Serializable]
    public class ABC_AudioClipReference {

        public AudioClip refVal;
        public string refName;
        public string refUpdateDateTime = string.Empty;

        public AudioClip AudioClip {
            //If null then get will retrieve the object in game by name recorded
            get {


                //If wrong refval was loaded and doesn't match the refname then reset the refval (unless *_ABC in name as that has been modified during play) 
                if (refVal != null && refVal.name != refName && refVal.name.Contains("*_ABC") == false)
                    refVal = null;

                //If refval is empty but we have a refname then find the val in the project using the name
                if (refVal == null && System.String.IsNullOrEmpty(refName) == false) {
                    List<AudioClip> allAudioClipObjects = new List<AudioClip>();

                    foreach (AudioClip go in Resources.FindObjectsOfTypeAll(typeof(AudioClip)) as AudioClip[]) {

                        allAudioClipObjects.Add(go);
                    }

                    //try and find the object in scene
                    refVal = allAudioClipObjects.Select(item => item).Where(obj => obj.name != null && obj.name == refName).FirstOrDefault();

                }

                return refVal;
            }

            //Set will record the value and the name, the name is used later to retrieve the object if needed
            set {
                refVal = value;
                refName = value.name;
            }
        }

        /// <summary>
        /// Refresh the custom reference making sure all values are up to date (refname matches refvalue)
        /// </summary>
        public void RefreshCustomReference() {
            //If refval exists and refname doesn't match the name of the value then update
            if (refVal != null && refVal.name != refName) {

                //update name
                refName = refVal.name;

                // update date time
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }

            //If data was cleared then clear the string value also 
            if (refVal == null && string.IsNullOrEmpty(refName) == false) {

                //Blank name
                refName = "";

                //Update time the reference was last updated
                refUpdateDateTime = System.DateTime.UtcNow.ToString();
            }
        }

    }
}



