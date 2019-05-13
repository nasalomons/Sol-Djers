using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour {
    private GameObject meleeHotbar;
    private GameObject rangedHotbar;
    private SelectManager selectManager;

    public GameObject hotbar;
    public SelectableCharacter meleeChar;
    public SelectableCharacter rangedChar;

    // Start is called before the first frame update
    void Start() {
        meleeHotbar = hotbar.transform.GetChild(0).gameObject;
        SetUpHotbar(meleeChar, meleeHotbar);
        rangedHotbar = hotbar.transform.GetChild(1).gameObject;
        SetUpHotbar(rangedChar, rangedHotbar);
        selectManager = SelectManager.Instance;
    }

    // Update is called once per frame
    void Update() {
        if (selectManager.GetNumSelected() == 1) {
            hotbar.SetActive(true);
            GameObject currHotbar = null;
            SelectableCharacter currChar = null;
            if (meleeChar != null && meleeChar.GetSelected()) {
                currHotbar = meleeHotbar;
                currChar = meleeChar;
                rangedHotbar.SetActive(false);
            } else if (rangedChar != null && rangedChar.GetSelected()) {
                currHotbar = rangedHotbar;
                currChar = rangedChar;
                meleeHotbar.SetActive(false);
            }

            if (currChar != null && currHotbar != null) {
                currHotbar.SetActive(true);

                Ability[] abilities = currChar.GetAbilities();
                for (int i = 0; i < abilities.Length; i++) {
                    Ability ability = abilities[i];
                    if (ability.Cooldown() > 0) {
                        GameObject abilityCd = currHotbar.transform.GetChild(i).GetChild(0).gameObject;
                        abilityCd.SetActive(true);
                        abilityCd.transform.GetComponentInChildren<Text>().text = ability.Cooldown() + "";
                    } else {
                        currHotbar.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
                    }
                }
            } else {
                hotbar.SetActive(false);
            }              
        } else {
            hotbar.SetActive(false);
        }  
    }

    private void SetUpHotbar(SelectableCharacter character, GameObject hotbar) {
        Button[] buttons = hotbar.GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(delegate {
            ButtonTask(character, 0);
        });
        buttons[1].onClick.AddListener(delegate {
            ButtonTask(character, 1);
        });
        buttons[2].onClick.AddListener(delegate {
            ButtonTask(character, 2);
        });


    }

    private void ButtonTask(SelectableCharacter character, int abilityIndex) {
        character.PrepareAbility(abilityIndex);
        character.HotbarClicked();
    }
}
