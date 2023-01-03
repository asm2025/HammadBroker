import Button from 'primevue/button';
import TieredMenu from 'primevue/tieredmenu';
import { UniqueComponentId } from 'primevue/utils';
import { resolveComponent, openBlock, createElementBlock, normalizeClass, normalizeStyle, renderSlot, createVNode, mergeProps } from 'vue';

var script = {
    name: 'SplitButton',
    props: {
        label: {
            type: String,
            default: null
        },
        icon: {
            type: String,
            default: null
        },
        model: {
            type: Array,
            default: null
        },
        autoZIndex: {
            type: Boolean,
            default: true
        },
        baseZIndex: {
            type: Number,
            default: 0
        },
        appendTo: {
            type: String,
            default: 'body'
        },
        disabled: {
            type: Boolean,
            default: false
        },
        class: {
            type: null,
            default: null
        },
        style: {
            type: null,
            default: null
        },
        buttonProps: {
            type: null,
            default: null
        },
        menuButtonProps: {
            type: null,
            default: null
        }
    },
    data() {
        return {
            isExpanded: false
        };
    },
    methods: {
        onDropdownButtonClick() {
            this.$refs.menu.toggle({ currentTarget: this.$el, relatedTarget: this.$refs.button.$el });
            this.isExpanded = !this.$refs.menu.visible;
        },
        onDropdownKeydown(event) {
            if (event.code === 'ArrowDown' || event.code === 'ArrowUp') {
                this.onDropdownButtonClick();
                event.preventDefault();
            }
        },
        onDefaultButtonClick(event) {
            this.$refs.menu.hide(event);
        }
    },
    computed: {
        ariaId() {
            return UniqueComponentId();
        },
        containerClass() {
            return ['p-splitbutton p-component', this.class];
        }
    },
    components: {
        PVSButton: Button,
        PVSMenu: TieredMenu
    }
};

function render(_ctx, _cache, $props, $setup, $data, $options) {
  const _component_PVSButton = resolveComponent("PVSButton");
  const _component_PVSMenu = resolveComponent("PVSMenu");

  return (openBlock(), createElementBlock("div", {
    class: normalizeClass($options.containerClass),
    style: normalizeStyle($props.style)
  }, [
    renderSlot(_ctx.$slots, "default", {}, () => [
      createVNode(_component_PVSButton, mergeProps({
        type: "button",
        class: "p-splitbutton-defaultbutton",
        icon: $props.icon,
        label: $props.label,
        disabled: $props.disabled,
        "aria-label": $props.label,
        onClick: $options.onDefaultButtonClick
      }, $props.buttonProps), null, 16, ["icon", "label", "disabled", "aria-label", "onClick"])
    ]),
    createVNode(_component_PVSButton, mergeProps({
      ref: "button",
      type: "button",
      class: "p-splitbutton-menubutton",
      icon: "pi pi-chevron-down",
      disabled: $props.disabled,
      "aria-haspopup": "true",
      "aria-expanded": $data.isExpanded,
      "aria-controls": $options.ariaId + '_overlay',
      onClick: $options.onDropdownButtonClick,
      onKeydown: $options.onDropdownKeydown
    }, $props.menuButtonProps), null, 16, ["disabled", "aria-expanded", "aria-controls", "onClick", "onKeydown"]),
    createVNode(_component_PVSMenu, {
      ref: "menu",
      id: $options.ariaId + '_overlay',
      model: $props.model,
      popup: true,
      autoZIndex: $props.autoZIndex,
      baseZIndex: $props.baseZIndex,
      appendTo: $props.appendTo
    }, null, 8, ["id", "model", "autoZIndex", "baseZIndex", "appendTo"])
  ], 6))
}

function styleInject(css, ref) {
  if ( ref === void 0 ) ref = {};
  var insertAt = ref.insertAt;

  if (!css || typeof document === 'undefined') { return; }

  var head = document.head || document.getElementsByTagName('head')[0];
  var style = document.createElement('style');
  style.type = 'text/css';

  if (insertAt === 'top') {
    if (head.firstChild) {
      head.insertBefore(style, head.firstChild);
    } else {
      head.appendChild(style);
    }
  } else {
    head.appendChild(style);
  }

  if (style.styleSheet) {
    style.styleSheet.cssText = css;
  } else {
    style.appendChild(document.createTextNode(css));
  }
}

var css_248z = "\n.p-splitbutton[data-v-ea86a410] {\n    display: -webkit-inline-box;\n    display: -ms-inline-flexbox;\n    display: inline-flex;\n    position: relative;\n}\n.p-splitbutton .p-splitbutton-defaultbutton[data-v-ea86a410],\n.p-splitbutton.p-button-rounded > .p-splitbutton-defaultbutton.p-button[data-v-ea86a410],\n.p-splitbutton.p-button-outlined > .p-splitbutton-defaultbutton.p-button[data-v-ea86a410] {\n    -webkit-box-flex: 1;\n        -ms-flex: 1 1 auto;\n            flex: 1 1 auto;\n    border-top-right-radius: 0;\n    border-bottom-right-radius: 0;\n    border-right: 0 none;\n}\n.p-splitbutton-menubutton[data-v-ea86a410],\n.p-splitbutton.p-button-rounded > .p-splitbutton-menubutton.p-button[data-v-ea86a410],\n.p-splitbutton.p-button-outlined > .p-splitbutton-menubutton.p-button[data-v-ea86a410] {\n    display: -webkit-box;\n    display: -ms-flexbox;\n    display: flex;\n    -webkit-box-align: center;\n        -ms-flex-align: center;\n            align-items: center;\n    -webkit-box-pack: center;\n        -ms-flex-pack: center;\n            justify-content: center;\n    border-top-left-radius: 0;\n    border-bottom-left-radius: 0;\n}\n.p-splitbutton .p-menu[data-v-ea86a410] {\n    min-width: 100%;\n}\n.p-fluid .p-splitbutton[data-v-ea86a410] {\n    display: -webkit-box;\n    display: -ms-flexbox;\n    display: flex;\n}\n";
styleInject(css_248z);

script.render = render;
script.__scopeId = "data-v-ea86a410";

export { script as default };
