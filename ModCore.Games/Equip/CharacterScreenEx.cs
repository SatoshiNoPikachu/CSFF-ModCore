using ModCore.UI;
using TMPro;
using UnityEngine;

namespace ModCore.Games.Equip;

public class CharacterScreenEx : MBSingleton<CharacterScreenEx>
{
    private static readonly List<EquipTab> Tabs = EquipTab.Tabs;

    private readonly IndexButton[] _tabButtons = new IndexButton[Tabs.Count + 4];

    private CharacterScreen _screen = null!;

    private int _curTabPage = -1;

    private const int PageSize = 7;

    public static void Create(CharacterScreen screen)
    {
        if (!screen) return;

        var title = screen.transform.Find("DarkBG/ShadowAndPopupWithTitle/Content/Title");
        if (title is null) return;

        title.Find("Tabs").LocalX = 330;

        var titleText = (RectTransform)screen.TitleText.transform;
        titleText.sizeDelta = new Vector2(-1175, 0);
        titleText.LocalX = 12;

        var titleTmp = screen.TitleText.GetComponent<TextMeshProUGUI>();
        titleTmp.enableAutoSizing = true;
        titleTmp.fontSizeMax = 48;

        var nextBtnPrefab = UIManager.GetPrefab<ActionButton>(CommonPrefab.UidActionButton);
        if (nextBtnPrefab is null) return;

        var nextBtn = Instantiate(nextBtnPrefab, title, false);
        nextBtn.name = "NextTabButton";
        nextBtn.Text = "→";

        var nextBtnRt = (RectTransform)nextBtn.transform;
        nextBtnRt.anchorMin = nextBtnRt.anchorMax = nextBtnRt.pivot = Vector2.one;
        nextBtnRt.anchoredPosition = new Vector2(-95, -10);

        var tabPrefab = UIManager.GetPrefab<IndexButton>("ModCore:EquipTabButton");
        if (tabPrefab is null)
        {
            tabPrefab = Instantiate(screen.EquipmentTabButton);
            tabPrefab.name = "EquipExTabButton";
            UIManager.RegisterPrefab("ModCore:EquipTabButton", tabPrefab);
        }

        var tabsParent = screen.EquipmentTabButton.transform.parent;

        var ex = screen.gameObject.AddComponent<CharacterScreenEx>();
        ex._screen = screen;
        nextBtn.OnClick = ex.NextTabPage;

        var tabButtons = ex._tabButtons;
        tabButtons[0] = screen.EquipmentTabButton;
        tabButtons[1] = screen.WoundsTabButton;
        tabButtons[2] = screen.MagicTabButton;
        tabButtons[3] = screen.CharacterTabButton;
        for (var i = 4; i < tabButtons.Length; i++)
        {
            var tab = Tabs[i - 4];
            var tabBtn = Instantiate(tabPrefab, tabsParent, false);
            tabBtn.Setup(i, "", tab.TabName, false);
            tabBtn.Selected = false;
            tabBtn.OnClicked = ex.ChangeTab;
            if (tab.TabIcon) tabBtn.Sprite = tab.TabIcon;
            tabButtons[i] = tabBtn;
        }

        ex.ChangeTabPageToCur();
    }

    public void ChangeTab(int index)
    {
        if (index >= Tabs.Count + 4) return;

        if (gameObject.activeInHierarchy)
        {
            _screen.WoundsTabButton.NewNotification = _screen.HasAnyNewWounds;
            _screen.MagicTabButton.NewNotification = _screen.HasAnyNewMagic;
            _screen.CheckCards();
        }

        _screen.CurrentTab = index;
        foreach (var button in _tabButtons)
        {
            button.Selected = button.Index == index;
        }

        var tab = index >= 4 ? Tabs[index - 4] : null;

        _screen.TitleText.text = index switch
        {
            0 => LocalizedString.Equipment,
            1 => LocalizedString.Wounds,
            2 => LocalizedString.Magic,
            3 => LocalizedString.Character,
            _ => tab!.TabName
        };

        if (index == 3)
        {
            _screen.UpdateInRunPerks();
            _screen.EquipmentAndWoundsGroup.SetActive(false);
            _screen.CharacterSheetGroup.SetActive(true);
            return;
        }

        var line = _screen.EquipmentSlotsLine;
        foreach (var slot in line.Slots)
        {
            if (!slot.AssignedCard) continue;

            var card = slot.AssignedCard.CardModel;
            if (_screen.CardIsWound(card))
            {
                slot.IsActive = index == 1;
            }
            else if (_screen.CardIsMagic(card))
            {
                slot.IsActive = index == 2;
            }
            else if (tab?.IsThis(card) is true)
            {
                slot.IsActive = true;
            }
            else
            {
                slot.IsActive = index == 0 && EquipTab.IsEquipment(card);
            }
        }

        if (index > 0) line.MoveToPos(0);

        _screen.EquipmentAndWoundsGroup.SetActive(true);
        _screen.CharacterSheetGroup.SetActive(false);
    }

    public void Open()
    {
        Game.Grm!.CloseAllPopups(true);
        gameObject.SetActive(true);

        if (!_screen.EquipmentSlotsLine) return;

        var curTab = _screen.CurrentTab;
        var slot = GetNewCard();
        var card = slot?.AssignedCard.CardModel;
        if (card is not null)
        {
            if (_screen.CardIsWound(card))
            {
                ChangeTab(1);
            }
            else if (_screen.CardIsMagic(card))
            {
                ChangeTab(2);
            }
            else if (EquipTab.IsEquipment(card))
            {
                ChangeTab(0);
            }
            else
            {
                for (var i = 0; i < Tabs.Count; i++)
                {
                    var tab = Tabs[i];
                    if (!tab.IsThis(card)) continue;
                    ChangeTab(i + 4);
                    break;
                }
            }

            ChangeTabPageToCur();
            _screen.EquipmentSlotsLine.MoveViewTo(slot, true, true);
            return;
        }

        if (curTab == 3 && _screen.DontStayOnCharacterSheet)
        {
            ChangeTab(0);
            ChangeTabPageToCur();
            return;
        }

        ChangeTab(curTab);
        ChangeTabPageToCur();
    }

    private void ChangeTabPage()
    {
        var min = _curTabPage * PageSize;
        var max = min + PageSize;

        for (var i = 0; i < _tabButtons.Length; i++)
        {
            _tabButtons[i].gameObject.SetActive(i >= min && i < max);
        }
    }

    private void NextTabPage()
    {
        _curTabPage++;
        if (_curTabPage * PageSize >= _tabButtons.Length) _curTabPage = 0;

        ChangeTabPage();
    }

    private void ChangeTabPageToCur()
    {
        var page = _screen.CurrentTab / PageSize;
        if (page == _curTabPage) return;

        _curTabPage = page;
        ChangeTabPage();
    }

    private DynamicLayoutSlot? GetNewCard()
    {
        var slots = _screen.EquipmentSlotsLine.Slots;
        if (slots is null) return null;

        foreach (var slot in slots)
        {
            var card = slot.AssignedCard.Val;
            if (card?.CardModel?.IsImportantEquipment is not true) continue;
            if (_screen.CheckedCards.Contains(card)) continue;

            return slot;
        }

        return null;
    }
}