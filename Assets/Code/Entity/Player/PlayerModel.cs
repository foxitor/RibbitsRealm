using System.Collections; using UnityEngine; using UnityEngine.UI;

public class PlayerModel : MonoBehaviour {
    [SerializeField]
    GameObject firstPersonRoot;
    PlayerController player;
    ItemLib itemStorage;

    ItemStack PreviousHeldItem;
    GameObject ItemInHand;

    public GameObject Hotbar;
    RectTransform SelectedSlotDisplay;
    RectTransform SlotContents;
    int PreviousHotbarSlot;
    public ItemStack[] PreviousHotbarState;

    Vector2 moveInput; 

    GameObject fpMainHand;
    GameObject fpMainHandHolder;
    Transform fpMainHandStart;

    GameObject fpOffHand;
    Transform fpOffHandStart;

    void Start() {
        player = transform.parent.GetComponent<PlayerController>();
        itemStorage = GameObject.Find("Libs/ItemLib").GetComponent<ItemLib>();

        firstPersonRoot.transform.parent.gameObject.SetActive(false);
        firstPersonRoot.transform.parent.gameObject.SetActive(true);

        fpMainHand = firstPersonRoot.transform.GetChild(0).gameObject;
        fpMainHandHolder = fpMainHand.transform.GetChild(0).GetChild(0).gameObject;
        fpMainHandStart = fpMainHand.transform;
        fpOffHand = firstPersonRoot.transform.GetChild(1).gameObject;
        fpOffHandStart = fpOffHand.transform;
        SelectedSlotDisplay = Hotbar.transform.GetChild(0).GetComponent<RectTransform>();
        SlotContents = Hotbar.transform.GetChild(1).GetComponent<RectTransform>();
    }
    void Update() {
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        float mouseX = Input.GetAxis("Mouse X") * 5;
        float mouseY = Input.GetAxis("Mouse Y") * 5;

        float MoveY = player.gameObject.GetComponent<CharacterController>().velocity.y * 5 * (player.runState ? 2 : 1);

        Quaternion rotationX = Quaternion.AngleAxis(mouseY + MoveY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(-mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        fpMainHand.transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, 5 * Time.deltaTime);
        fpOffHand.transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, 5 * Time.deltaTime);

        fpMainHand.GetComponent<Animator>().SetBool("walking", moveInput != Vector2.zero);
        fpOffHand.GetComponent<Animator>().SetBool("walking", moveInput != Vector2.zero);

        if (PreviousHeldItem != player.SelectedItem) {
            Destroy(ItemInHand);
            if (itemStorage.DisplayItem(player.HotbarItems[player.SelectedSlot].id) != null) {
                ItemInHand = Instantiate(itemStorage.DisplayItem(player.HotbarItems[player.SelectedSlot].id), 
                    fpMainHandHolder.transform.position, new Quaternion(), 
                            fpMainHandHolder.transform);
            }

            PreviousHeldItem = player.SelectedItem;
        }
        if (PreviousHotbarSlot != player.SelectedSlot) {
            SelectedSlotDisplay.anchoredPosition = new Vector2(56.25f + (93.75f * player.SelectedSlot), 3);
            PreviousHotbarSlot = player.SelectedSlot;
        }
        if (PreviousHotbarState != player.HotbarItems) {
            for (int i = 0; i < 9; i++) {
                if (itemStorage.ShareRawItem(player.HotbarItems[i].id) != null) {
                    SlotContents.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    SlotContents.GetChild(i).GetComponent<Image>().sprite = itemStorage.ShareRawItem(player.HotbarItems[i].id).icon;
                    if (player.HotbarItems[i].count > 1) {
                        SlotContents.GetChild(i).GetChild(0).GetComponent<Text>().text = player.HotbarItems[i].count.ToString();
                    } else {
                        SlotContents.GetChild(i).GetChild(0).GetComponent<Text>().text = "";
                    }
                } else {
                    SlotContents.GetChild(i).GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    SlotContents.GetChild(i).GetChild(0).GetComponent<Text>().text = "";
                }
            }
            PreviousHotbarState = player.HotbarItems;
        }
    }
    public void punch() {
        Animator anim = fpMainHand.GetComponent<Animator>();
        if (player.gameObject.GetComponent<CharacterController>().velocity.y >= 0) {
            anim.ResetTrigger("punch");
            anim.SetTrigger("punch");
        } else {
            anim.ResetTrigger("crit");
            anim.SetTrigger("crit");
        }
    }
    public void interact() {
        Animator anim = fpMainHand.GetComponent<Animator>();
        anim.ResetTrigger("interact");
        anim.SetTrigger("interact");
    }
}
