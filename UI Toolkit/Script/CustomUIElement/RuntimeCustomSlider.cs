using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RuntimeCustomSlider : MonoBehaviour
{
    private VisualElement root;
    private List<VisualElement> slider;
    private List<VisualElement> dragger;
    private List<VisualElement> bar;
    private List<VisualElement> newDragger;

    private void Start()
    {
        slider = new List<VisualElement>();
        dragger = new List<VisualElement>();
        bar = new List<VisualElement>();
        newDragger = new List<VisualElement>();

        root = GetComponent<UIDocument>().rootVisualElement;
        slider = root.Query<VisualElement>("MySlider").ToList();
       
        // find all slider elements

        for (int i = 0; i < slider.Count; i++)
        {
            VisualElement mySlider = slider[i];
            dragger.Add(mySlider.Q<VisualElement>("unity-dragger"));
            bar.Add(mySlider.Q<VisualElement>("Bar"));
            
            AddElements(mySlider, dragger[i]);

            // i를 그대로 넣으면 i의 마지막 값으로 callback이 실행되므로 index 변수를 만들어서 i를 넣어줌
            // 왜?
            int index = i;

            mySlider.RegisterCallback<ChangeEvent<float>>(
                (ChangeEvent<float> evt) => SliderValueChanged(evt, index));
            mySlider.RegisterCallback<GeometryChangedEvent>(
                (GeometryChangedEvent evt) => SliderInit(evt, index));
        }
    }

    private void AddElements(VisualElement slider, VisualElement dragger)
    {
        VisualElement bar = new VisualElement();
        dragger.Add(bar);
        bar.name = "Bar";
        bar.AddToClassList("bar");

        VisualElement newDragger = new VisualElement();
        slider.Add(newDragger);
        newDragger.name = "NewDragger";
        newDragger.AddToClassList("newdragger");
        newDragger.pickingMode = PickingMode.Ignore;

        this.bar.Add(bar);
        this.newDragger.Add(newDragger);
    }

    private void SliderValueChanged(ChangeEvent<float> evt, int i)
    {
        Vector2 dist = new Vector2((newDragger[i].layout.width - dragger[i].layout.width) / 2, 
            (newDragger[i].layout.height - dragger[i].layout.height) / 2);
        Vector2 pos = dragger[i].parent.LocalToWorld(dragger[i].transform.position);
        newDragger[i].transform.position = newDragger[i].parent.WorldToLocal(pos - dist);
    }

    private void SliderInit(GeometryChangedEvent evt, int i)
    {
        Vector2 dist = new Vector2((newDragger[i].layout.width - dragger[i].layout.width) / 2,
             (newDragger[i].layout.height - dragger[i].layout.height) / 2);
        Vector2 pos = dragger[i].parent.LocalToWorld(dragger[i].transform.position);
        newDragger[i].transform.position = newDragger[i].parent.WorldToLocal(pos - dist);
    }

}
