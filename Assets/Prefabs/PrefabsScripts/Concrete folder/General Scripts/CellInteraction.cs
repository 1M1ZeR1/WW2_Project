using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFx.Outline;

public class CellInteraction
{
    [SerializeField] private Material onEnterMaterial;
    [SerializeField] private Material onExitMaterial;

    [SerializeField] private Material selectedStageMaterial_MouseOnObject;
    [SerializeField] private Material selectedStageMaterial_MouseOffObject;

    [SerializeField] private Material onEnterMaterial_Allies;
    [SerializeField] private Material onExitMaterial_Allies;

    [SerializeField] private Material selectedStageMaterial_MouseOnObject_Allies;
    [SerializeField] private Material selectedStageMaterial_MouseOffObject_Allies;

    [SerializeField] private Material onEnterMaterial_Enemys;
    [SerializeField] private Material onExitMaterial_Enemys;

    [SerializeField] private Material selectedStageMaterial_MouseOnObject_Enemys;
    [SerializeField] private Material selectedStageMaterial_MouseOffObject_Enemys;

    protected GameObject _cellInterectWith;
    protected GameObject _lastRenderer;


    protected CellTypeScript cellTypeScript;
    
    public CellInteraction()
    {
        ServiceRegistry.WorkWithService<EventBus>().Subscribe<InteractableScript, GameObject>((sender, cell) =>
        {
            PlayerInteractWithGameObject(cell);
        });

        onEnterMaterial = Resources.Load<Material>("MapMaterials/None side materials/Enter");
        onExitMaterial = Resources.Load<Material>("MapMaterials/None side materials/Exit");

        selectedStageMaterial_MouseOnObject = Resources.Load<Material>("MapMaterials/None side materials/Enter Selected");
        selectedStageMaterial_MouseOffObject = Resources.Load<Material>("MapMaterials/None side materials/Exit selected");


        onEnterMaterial_Allies = Resources.Load<Material>("MapMaterials/Allies materials/Enter");
        onExitMaterial_Allies = Resources.Load<Material>("MapMaterials/Allies materials/Exit");

        selectedStageMaterial_MouseOnObject_Allies = Resources.Load<Material>("MapMaterials/Allies materials/Enter Selected");
        selectedStageMaterial_MouseOffObject_Allies = Resources.Load<Material>("MapMaterials/Allies materials/Exit selected");


        onEnterMaterial_Enemys = Resources.Load<Material>("MapMaterials/Enemys materials/Enter");
        onExitMaterial_Enemys = Resources.Load<Material>("MapMaterials/Enemys materials/Exit");

        selectedStageMaterial_MouseOnObject_Enemys = Resources.Load<Material>("MapMaterials/Enemys materials/Enter Selected");
        selectedStageMaterial_MouseOffObject_Enemys = Resources.Load<Material>("MapMaterials/Enemys materials/Exit selected");
    }

    private void PlayerInteractWithGameObject(GameObject interableGameObject)
    {
        if (interableGameObject == _cellInterectWith)
        {
            SetMaterialBySide_Interact
                (
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(interableGameObject).GetParameter<CellArea>().Side,
                interableGameObject,
                false
                );
        }
        else
        {
            if (_cellInterectWith != null)
            {
                var buff = _cellInterectWith;
                _cellInterectWith = null;

                SetMaterialBySide_Basic
                    (
                    ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(buff).GetParameter<CellArea>().Side,
                    buff,
                    false
                    );
            }

            _cellInterectWith = interableGameObject;

            SetMaterialBySide_Interact
                (
                ServiceRegistry.WorkWithController<CellController>().WorkWithCell<CellParametersHandler>(interableGameObject).GetParameter<CellArea>().Side,
                interableGameObject,
                true
                );
        }
    }

    public void SetMaterialBySide_Basic(SideEnum side, GameObject cell, bool stage)
    {
        if(cell == _cellInterectWith) { return; }
        MeshRenderer meshRenderer = cell.GetComponent<MeshRenderer>();

        if (stage)
        {
            switch (side)
            {
                case SideEnum.None: meshRenderer.material = onEnterMaterial; break;
                case SideEnum.Allies: meshRenderer.material = onEnterMaterial_Allies; break;
                case SideEnum.Enemys: meshRenderer.material = onEnterMaterial_Enemys; break;
            }
        }
        else 
        {
            switch (side)
            {
                case SideEnum.None: meshRenderer.material = onExitMaterial; break;
                case SideEnum.Allies: meshRenderer.material = onExitMaterial_Allies; break;
                case SideEnum.Enemys: meshRenderer.material = onExitMaterial_Enemys; break;
            }
        }
    }
    public void SetMaterialBySide_Interact(SideEnum side, GameObject cell, bool stage)
    {
        MeshRenderer meshRenderer = cell.GetComponent<MeshRenderer>();

        if (stage)
        {
            switch (side)
            {
                case SideEnum.None: meshRenderer.material = selectedStageMaterial_MouseOnObject; break;
                case SideEnum.Allies: meshRenderer.material = selectedStageMaterial_MouseOnObject_Allies; break;
                case SideEnum.Enemys: meshRenderer.material = selectedStageMaterial_MouseOnObject_Enemys; break;
            }
        }
        else
        {
            switch (side)
            {
                case SideEnum.None: meshRenderer.material = selectedStageMaterial_MouseOffObject; break;
                case SideEnum.Allies: meshRenderer.material = selectedStageMaterial_MouseOffObject_Allies; break;
                case SideEnum.Enemys: meshRenderer.material = selectedStageMaterial_MouseOffObject_Enemys; break;
            }
        }
    }
}
