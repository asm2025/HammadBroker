import { FilterService } from 'primevue/api';
import OverlayEventBus from 'primevue/overlayeventbus';
import Portal from 'primevue/portal';
import Ripple from 'primevue/ripple';
import { ZIndexUtils, ObjectUtils, DomHandler, ConnectedOverlayScrollHandler, UniqueComponentId } from 'primevue/utils';
import VirtualScroller from 'primevue/virtualscroller';
import { resolveComponent, resolveDirective, openBlock, createElementBlock, normalizeClass, mergeProps, renderSlot, createTextVNode, toDisplayString, createCommentVNode, createElementVNode, createVNode, withCtx, Transition, normalizeStyle, createSlots, Fragment, renderList, withDirectives } from 'vue';

var script = {
    name: 'Dropdown',
    emits: ['update:modelValue', 'change', 'focus', 'blur', 'before-show', 'before-hide', 'show', 'hide', 'filter'],
    props: {
        modelValue: null,
        options: Array,
        optionLabel: null,
        optionValue: null,
        optionDisabled: null,
        optionGroupLabel: null,
        optionGroupChildren: null,
        scrollHeight: {
            type: String,
            default: '200px'
        },
        filter: Boolean,
        filterPlaceholder: String,
        filterLocale: String,
        filterMatchMode: {
            type: String,
            default: 'contains'
        },
        filterFields: {
            type: Array,
            default: null
        },
        editable: Boolean,
        placeholder: {
            type: String,
            default: null
        },
        disabled: {
            type: Boolean,
            default: false
        },
        dataKey: null,
        showClear: {
            type: Boolean,
            default: false
        },
        inputId: {
            type: String,
            default: null
        },
        inputClass: {
            type: String,
            default: null
        },
        inputStyle: {
            type: null,
            default: null
        },
        inputProps: {
            type: null,
            default: null
        },
        panelClass: {
            type: String,
            default: null
        },
        panelStyle: {
            type: null,
            default: null
        },
        panelProps: {
            type: null,
            default: null
        },
        filterInputProps: {
            type: null,
            default: null
        },
        clearIconProps: {
            type: null,
            default: null
        },
        appendTo: {
            type: String,
            default: 'body'
        },
        loading: {
            type: Boolean,
            default: false
        },
        clearIcon: {
            type: String,
            default: 'pi pi-times'
        },
        dropdownIcon: {
            type: String,
            default: 'pi pi-chevron-down'
        },
        filterIcon: {
            type: String,
            default: 'pi pi-search'
        },
        loadingIcon: {
            type: String,
            default: 'pi pi-spinner pi-spin'
        },
        resetFilterOnHide: {
            type: Boolean,
            default: false
        },
        virtualScrollerOptions: {
            type: Object,
            default: null
        },
        autoOptionFocus: {
            type: Boolean,
            default: true
        },
        autoFilterFocus: {
            type: Boolean,
            default: false
        },
        selectOnFocus: {
            type: Boolean,
            default: false
        },
        filterMessage: {
            type: String,
            default: null
        },
        selectionMessage: {
            type: String,
            default: null
        },
        emptySelectionMessage: {
            type: String,
            default: null
        },
        emptyFilterMessage: {
            type: String,
            default: null
        },
        emptyMessage: {
            type: String,
            default: null
        },
        tabindex: {
            type: Number,
            default: 0
        },
        'aria-label': {
            type: String,
            default: null
        },
        'aria-labelledby': {
            type: String,
            default: null
        }
    },
    outsideClickListener: null,
    scrollHandler: null,
    resizeListener: null,
    overlay: null,
    list: null,
    virtualScroller: null,
    searchTimeout: null,
    searchValue: null,
    isModelValueChanged: false,
    focusOnHover: false,
    data() {
        return {
            focused: false,
            focusedOptionIndex: -1,
            filterValue: null,
            overlayVisible: false
        };
    },
    watch: {
        modelValue() {
            this.isModelValueChanged = true;
        },
        options() {
            this.autoUpdateModel();
        }
    },
    mounted() {
        this.autoUpdateModel();
    },
    updated() {
        if (this.overlayVisible && this.isModelValueChanged) {
            this.scrollInView(this.findSelectedOptionIndex());
        }

        this.isModelValueChanged = false;
    },
    beforeUnmount() {
        this.unbindOutsideClickListener();
        this.unbindResizeListener();

        if (this.scrollHandler) {
            this.scrollHandler.destroy();
            this.scrollHandler = null;
        }

        if (this.overlay) {
            ZIndexUtils.clear(this.overlay);
            this.overlay = null;
        }
    },
    methods: {
        getOptionIndex(index, fn) {
            return this.virtualScrollerDisabled ? index : fn && fn(index)['index'];
        },
        getOptionLabel(option) {
            return this.optionLabel ? ObjectUtils.resolveFieldData(option, this.optionLabel) : option;
        },
        getOptionValue(option) {
            return this.optionValue ? ObjectUtils.resolveFieldData(option, this.optionValue) : option;
        },
        getOptionRenderKey(option, index) {
            return (this.dataKey ? ObjectUtils.resolveFieldData(option, this.dataKey) : this.getOptionLabel(option)) + '_' + index;
        },
        isOptionDisabled(option) {
            return this.optionDisabled ? ObjectUtils.resolveFieldData(option, this.optionDisabled) : false;
        },
        isOptionGroup(option) {
            return this.optionGroupLabel && option.optionGroup && option.group;
        },
        getOptionGroupLabel(optionGroup) {
            return ObjectUtils.resolveFieldData(optionGroup, this.optionGroupLabel);
        },
        getOptionGroupChildren(optionGroup) {
            return ObjectUtils.resolveFieldData(optionGroup, this.optionGroupChildren);
        },
        getAriaPosInset(index) {
            return (this.optionGroupLabel ? index - this.visibleOptions.slice(0, index).filter((option) => this.isOptionGroup(option)).length : index) + 1;
        },
        show(isFocus) {
            this.$emit('before-show');
            this.overlayVisible = true;
            this.focusedOptionIndex = this.focusedOptionIndex !== -1 ? this.focusedOptionIndex : this.autoOptionFocus ? this.findFirstFocusedOptionIndex() : -1;

            isFocus && DomHandler.focus(this.$refs.focusInput);
        },
        hide(isFocus) {
            const _hide = () => {
                this.$emit('before-hide');
                this.overlayVisible = false;
                this.focusedOptionIndex = -1;
                this.searchValue = '';

                this.resetFilterOnHide && (this.filterValue = null);
                isFocus && DomHandler.focus(this.$refs.focusInput);
            };

            setTimeout(() => {
                _hide();
            }, 0); // For ScreenReaders
        },
        onFocus(event) {
            this.focused = true;
            this.focusedOptionIndex = this.focusedOptionIndex !== -1 ? this.focusedOptionIndex : this.overlayVisible && this.autoOptionFocus ? this.findFirstFocusedOptionIndex() : -1;
            this.overlayVisible && this.scrollInView(this.focusedOptionIndex);
            this.$emit('focus', event);
        },
        onBlur(event) {
            this.focused = false;
            this.focusedOptionIndex = -1;
            this.searchValue = '';
            this.$emit('blur', event);
        },
        onKeyDown(event) {
            const metaKey = event.metaKey || event.ctrlKey;

            switch (event.code) {
                case 'ArrowDown':
                    this.onArrowDownKey(event);
                    break;

                case 'ArrowUp':
                    this.onArrowUpKey(event, this.editable);
                    break;

                case 'ArrowLeft':
                case 'ArrowRight':
                    this.onArrowLeftKey(event, this.editable);
                    break;

                case 'Home':
                    this.onHomeKey(event, this.editable);
                    break;

                case 'End':
                    this.onEndKey(event, this.editable);
                    break;

                case 'PageDown':
                    this.onPageDownKey(event);
                    break;

                case 'PageUp':
                    this.onPageUpKey(event);
                    break;

                case 'Space':
                    this.onSpaceKey(event, this.editable);
                    break;

                case 'Enter':
                case 'NumpadEnter':
                    this.onEnterKey(event);
                    break;

                case 'Escape':
                    this.onEscapeKey(event);
                    break;

                case 'Tab':
                    this.onTabKey(event);
                    break;

                case 'Backspace':
                    this.onBackspaceKey(event, this.editable);
                    break;

                case 'ShiftLeft':
                case 'ShiftRight':
                    //NOOP
                    break;

                default:
                    if (!metaKey && ObjectUtils.isPrintableCharacter(event.key)) {
                        !this.overlayVisible && this.show();
                        !this.editable && this.searchOptions(event, event.key);
                    }

                    break;
            }
        },
        onEditableInput(event) {
            const value = event.target.value;

            this.searchValue = '';
            const matched = this.searchOptions(event, value);

            !matched && (this.focusedOptionIndex = -1);

            this.$emit('update:modelValue', value);
        },
        onContainerClick(event) {
            if (this.disabled || this.loading) {
                return;
            }

            if (DomHandler.hasClass(event.target, 'p-dropdown-clear-icon') || event.target.tagName === 'INPUT') {
                return;
            } else if (!this.overlay || !this.overlay.contains(event.target)) {
                this.overlayVisible ? this.hide(true) : this.show(true);
            }
        },
        onClearClick(event) {
            this.updateModel(event, null);
        },
        onFirstHiddenFocus(event) {
            const focusableEl = event.relatedTarget === this.$refs.focusInput ? DomHandler.getFirstFocusableElement(this.overlay, ':not(.p-hidden-focusable)') : this.$refs.focusInput;

            DomHandler.focus(focusableEl);
        },
        onLastHiddenFocus(event) {
            const focusableEl = event.relatedTarget === this.$refs.focusInput ? DomHandler.getLastFocusableElement(this.overlay, ':not(.p-hidden-focusable)') : this.$refs.focusInput;

            DomHandler.focus(focusableEl);
        },
        onOptionSelect(event, option, isHide = true) {
            const value = this.getOptionValue(option);

            this.updateModel(event, value);
            isHide && this.hide(true);
        },
        onOptionMouseMove(event, index) {
            if (this.focusOnHover) {
                this.changeFocusedOptionIndex(event, index);
            }
        },
        onFilterChange(event) {
            const value = event.target.value;

            this.filterValue = value;
            this.focusedOptionIndex = -1;
            this.$emit('filter', { originalEvent: event, value });

            !this.virtualScrollerDisabled && this.virtualScroller.scrollToIndex(0);
        },
        onFilterKeyDown(event) {
            switch (event.code) {
                case 'ArrowDown':
                    this.onArrowDownKey(event);
                    break;

                case 'ArrowUp':
                    this.onArrowUpKey(event, true);
                    break;

                case 'ArrowLeft':
                case 'ArrowRight':
                    this.onArrowLeftKey(event, true);
                    break;

                case 'Home':
                    this.onHomeKey(event, true);
                    break;

                case 'End':
                    this.onEndKey(event, true);
                    break;

                case 'Enter':
                    this.onEnterKey(event);
                    break;

                case 'Escape':
                    this.onEscapeKey(event);
                    break;

                case 'Tab':
                    this.onTabKey(event, true);
                    break;
            }
        },
        onFilterBlur() {
            this.focusedOptionIndex = -1;
        },
        onFilterUpdated() {
            if (this.overlayVisible) {
                this.alignOverlay();
            }
        },
        onOverlayClick(event) {
            OverlayEventBus.emit('overlay-click', {
                originalEvent: event,
                target: this.$el
            });
        },
        onOverlayKeyDown(event) {
            switch (event.code) {
                case 'Escape':
                    this.onEscapeKey(event);
                    break;
            }
        },
        onArrowDownKey(event) {
            const optionIndex = this.focusedOptionIndex !== -1 ? this.findNextOptionIndex(this.focusedOptionIndex) : this.findFirstFocusedOptionIndex();

            this.changeFocusedOptionIndex(event, optionIndex);

            !this.overlayVisible && this.show();
            event.preventDefault();
        },
        onArrowUpKey(event, pressedInInputText = false) {
            if (event.altKey && !pressedInInputText) {
                if (this.focusedOptionIndex !== -1) {
                    this.onOptionSelect(event, this.visibleOptions[this.focusedOptionIndex]);
                }

                this.overlayVisible && this.hide();
                event.preventDefault();
            } else {
                const optionIndex = this.focusedOptionIndex !== -1 ? this.findPrevOptionIndex(this.focusedOptionIndex) : this.findLastFocusedOptionIndex();

                this.changeFocusedOptionIndex(event, optionIndex);

                !this.overlayVisible && this.show();
                event.preventDefault();
            }
        },
        onArrowLeftKey(event, pressedInInputText = false) {
            pressedInInputText && (this.focusedOptionIndex = -1);
        },
        onHomeKey(event, pressedInInputText = false) {
            if (pressedInInputText) {
                event.currentTarget.setSelectionRange(0, 0);
                this.focusedOptionIndex = -1;
            } else {
                this.changeFocusedOptionIndex(event, this.findFirstOptionIndex());

                !this.overlayVisible && this.show();
            }

            event.preventDefault();
        },
        onEndKey(event, pressedInInputText = false) {
            if (pressedInInputText) {
                const target = event.currentTarget;
                const len = target.value.length;

                target.setSelectionRange(len, len);
                this.focusedOptionIndex = -1;
            } else {
                this.changeFocusedOptionIndex(event, this.findLastOptionIndex());

                !this.overlayVisible && this.show();
            }

            event.preventDefault();
        },
        onPageUpKey(event) {
            this.scrollInView(0);
            event.preventDefault();
        },
        onPageDownKey(event) {
            this.scrollInView(this.visibleOptions.length - 1);
            event.preventDefault();
        },
        onEnterKey(event) {
            if (!this.overlayVisible) {
                this.onArrowDownKey(event);
            } else {
                if (this.focusedOptionIndex !== -1) {
                    this.onOptionSelect(event, this.visibleOptions[this.focusedOptionIndex]);
                }

                this.hide();
            }

            event.preventDefault();
        },
        onSpaceKey(event, pressedInInputText = false) {
            !pressedInInputText && this.onEnterKey(event);
        },
        onEscapeKey(event) {
            this.overlayVisible && this.hide(true);
            event.preventDefault();
        },
        onTabKey(event, pressedInInputText = false) {
            if (!pressedInInputText) {
                if (this.overlayVisible && this.hasFocusableElements()) {
                    DomHandler.focus(this.$refs.firstHiddenFocusableElementOnOverlay);

                    event.preventDefault();
                } else {
                    if (this.focusedOptionIndex !== -1) {
                        this.onOptionSelect(event, this.visibleOptions[this.focusedOptionIndex]);
                    }

                    this.overlayVisible && this.hide(this.filter);
                }
            }
        },
        onBackspaceKey(event, pressedInInputText = false) {
            if (pressedInInputText) {
                !this.overlayVisible && this.show();
            }
        },
        onOverlayEnter(el) {
            ZIndexUtils.set('overlay', el, this.$primevue.config.zIndex.overlay);
            this.alignOverlay();
            this.scrollInView();

            this.autoFilterFocus && DomHandler.focus(this.$refs.filterInput);
        },
        onOverlayAfterEnter() {
            this.bindOutsideClickListener();
            this.bindScrollListener();
            this.bindResizeListener();

            this.$emit('show');
        },
        onOverlayLeave() {
            this.unbindOutsideClickListener();
            this.unbindScrollListener();
            this.unbindResizeListener();

            this.$emit('hide');
            this.overlay = null;
        },
        onOverlayAfterLeave(el) {
            ZIndexUtils.clear(el);
        },
        alignOverlay() {
            if (this.appendTo === 'self') {
                DomHandler.relativePosition(this.overlay, this.$el);
            } else {
                this.overlay.style.minWidth = DomHandler.getOuterWidth(this.$el) + 'px';
                DomHandler.absolutePosition(this.overlay, this.$el);
            }
        },
        bindOutsideClickListener() {
            if (!this.outsideClickListener) {
                this.outsideClickListener = (event) => {
                    if (this.overlayVisible && this.overlay && !this.$el.contains(event.target) && !this.overlay.contains(event.target)) {
                        this.hide();
                    }
                };

                document.addEventListener('click', this.outsideClickListener);
            }
        },
        unbindOutsideClickListener() {
            if (this.outsideClickListener) {
                document.removeEventListener('click', this.outsideClickListener);
                this.outsideClickListener = null;
            }
        },
        bindScrollListener() {
            if (!this.scrollHandler) {
                this.scrollHandler = new ConnectedOverlayScrollHandler(this.$refs.container, () => {
                    if (this.overlayVisible) {
                        this.hide();
                    }
                });
            }

            this.scrollHandler.bindScrollListener();
        },
        unbindScrollListener() {
            if (this.scrollHandler) {
                this.scrollHandler.unbindScrollListener();
            }
        },
        bindResizeListener() {
            if (!this.resizeListener) {
                this.resizeListener = () => {
                    if (this.overlayVisible && !DomHandler.isTouchDevice()) {
                        this.hide();
                    }
                };

                window.addEventListener('resize', this.resizeListener);
            }
        },
        unbindResizeListener() {
            if (this.resizeListener) {
                window.removeEventListener('resize', this.resizeListener);
                this.resizeListener = null;
            }
        },
        hasFocusableElements() {
            return DomHandler.getFocusableElements(this.overlay, ':not(.p-hidden-focusable)').length > 0;
        },
        isOptionMatched(option) {
            return this.isValidOption(option) && this.getOptionLabel(option).toLocaleLowerCase(this.filterLocale).startsWith(this.searchValue.toLocaleLowerCase(this.filterLocale));
        },
        isValidOption(option) {
            return option && !(this.isOptionDisabled(option) || this.isOptionGroup(option));
        },
        isValidSelectedOption(option) {
            return this.isValidOption(option) && this.isSelected(option);
        },
        isSelected(option) {
            return ObjectUtils.equals(this.modelValue, this.getOptionValue(option), this.equalityKey);
        },
        findFirstOptionIndex() {
            return this.visibleOptions.findIndex((option) => this.isValidOption(option));
        },
        findLastOptionIndex() {
            return ObjectUtils.findLastIndex(this.visibleOptions, (option) => this.isValidOption(option));
        },
        findNextOptionIndex(index) {
            const matchedOptionIndex = index < this.visibleOptions.length - 1 ? this.visibleOptions.slice(index + 1).findIndex((option) => this.isValidOption(option)) : -1;

            return matchedOptionIndex > -1 ? matchedOptionIndex + index + 1 : index;
        },
        findPrevOptionIndex(index) {
            const matchedOptionIndex = index > 0 ? ObjectUtils.findLastIndex(this.visibleOptions.slice(0, index), (option) => this.isValidOption(option)) : -1;

            return matchedOptionIndex > -1 ? matchedOptionIndex : index;
        },
        findSelectedOptionIndex() {
            return this.hasSelectedOption ? this.visibleOptions.findIndex((option) => this.isValidSelectedOption(option)) : -1;
        },
        findFirstFocusedOptionIndex() {
            const selectedIndex = this.findSelectedOptionIndex();

            return selectedIndex < 0 ? this.findFirstOptionIndex() : selectedIndex;
        },
        findLastFocusedOptionIndex() {
            const selectedIndex = this.findSelectedOptionIndex();

            return selectedIndex < 0 ? this.findLastOptionIndex() : selectedIndex;
        },
        searchOptions(event, char) {
            this.searchValue = (this.searchValue || '') + char;

            let optionIndex = -1;
            let matched = false;

            if (this.focusedOptionIndex !== -1) {
                optionIndex = this.visibleOptions.slice(this.focusedOptionIndex).findIndex((option) => this.isOptionMatched(option));
                optionIndex = optionIndex === -1 ? this.visibleOptions.slice(0, this.focusedOptionIndex).findIndex((option) => this.isOptionMatched(option)) : optionIndex + this.focusedOptionIndex;
            } else {
                optionIndex = this.visibleOptions.findIndex((option) => this.isOptionMatched(option));
            }

            if (optionIndex !== -1) {
                matched = true;
            }

            if (optionIndex === -1 && this.focusedOptionIndex === -1) {
                optionIndex = this.findFirstFocusedOptionIndex();
            }

            if (optionIndex !== -1) {
                this.changeFocusedOptionIndex(event, optionIndex);
            }

            if (this.searchTimeout) {
                clearTimeout(this.searchTimeout);
            }

            this.searchTimeout = setTimeout(() => {
                this.searchValue = '';
                this.searchTimeout = null;
            }, 500);

            return matched;
        },
        changeFocusedOptionIndex(event, index) {
            if (this.focusedOptionIndex !== index) {
                this.focusedOptionIndex = index;
                this.scrollInView();

                if (this.selectOnFocus) {
                    this.onOptionSelect(event, this.visibleOptions[index], false);
                }
            }
        },
        scrollInView(index = -1) {
            const id = index !== -1 ? `${this.id}_${index}` : this.focusedOptionId;
            const element = DomHandler.findSingle(this.list, `li[id="${id}"]`);

            if (element) {
                element.scrollIntoView && element.scrollIntoView({ block: 'nearest', inline: 'start' });
            } else if (!this.virtualScrollerDisabled) {
                setTimeout(() => {
                    this.virtualScroller && this.virtualScroller.scrollToIndex(index !== -1 ? index : this.focusedOptionIndex);
                }, 0);
            }
        },
        autoUpdateModel() {
            if (this.selectOnFocus && this.autoOptionFocus && !this.hasSelectedOption) {
                this.focusedOptionIndex = this.findFirstFocusedOptionIndex();
                this.onOptionSelect(null, this.visibleOptions[this.focusedOptionIndex], false);
            }
        },
        updateModel(event, value) {
            this.$emit('update:modelValue', value);
            this.$emit('change', { originalEvent: event, value });
        },
        flatOptions(options) {
            return (options || []).reduce((result, option, index) => {
                result.push({ optionGroup: option, group: true, index });

                const optionGroupChildren = this.getOptionGroupChildren(option);

                optionGroupChildren && optionGroupChildren.forEach((o) => result.push(o));

                return result;
            }, []);
        },
        overlayRef(el) {
            this.overlay = el;
        },
        listRef(el, contentRef) {
            this.list = el;
            contentRef && contentRef(el); // For VirtualScroller
        },
        virtualScrollerRef(el) {
            this.virtualScroller = el;
        }
    },
    computed: {
        containerClass() {
            return [
                'p-dropdown p-component p-inputwrapper',
                {
                    'p-disabled': this.disabled,
                    'p-dropdown-clearable': this.showClear && !this.disabled,
                    'p-focus': this.focused,
                    'p-inputwrapper-filled': this.modelValue,
                    'p-inputwrapper-focus': this.focused || this.overlayVisible,
                    'p-overlay-open': this.overlayVisible
                }
            ];
        },
        inputStyleClass() {
            return [
                'p-dropdown-label p-inputtext',
                this.inputClass,
                {
                    'p-placeholder': !this.editable && this.label === this.placeholder,
                    'p-dropdown-label-empty': !this.editable && !this.$slots['value'] && (this.label === 'p-emptylabel' || this.label.length === 0)
                }
            ];
        },
        panelStyleClass() {
            return [
                'p-dropdown-panel p-component',
                this.panelClass,
                {
                    'p-input-filled': this.$primevue.config.inputStyle === 'filled',
                    'p-ripple-disabled': this.$primevue.config.ripple === false
                }
            ];
        },
        dropdownIconClass() {
            return ['p-dropdown-trigger-icon', this.loading ? this.loadingIcon : this.dropdownIcon];
        },
        visibleOptions() {
            const options = this.optionGroupLabel ? this.flatOptions(this.options) : this.options || [];

            if (this.filterValue) {
                const filteredOptions = FilterService.filter(options, this.searchFields, this.filterValue, this.filterMatchMode, this.filterLocale);

                if (this.optionGroupLabel) {
                    const optionGroups = this.options || [];
                    const filtered = [];

                    optionGroups.forEach((group) => {
                        const filteredItems = group.items.filter((item) => filteredOptions.includes(item));

                        if (filteredItems.length > 0) filtered.push({ ...group, items: [...filteredItems] });
                    });

                    return this.flatOptions(filtered);
                }

                return filteredOptions;
            }

            return options;
        },
        hasSelectedOption() {
            return ObjectUtils.isNotEmpty(this.modelValue);
        },
        label() {
            const selectedOptionIndex = this.findSelectedOptionIndex();

            return selectedOptionIndex !== -1 ? this.getOptionLabel(this.visibleOptions[selectedOptionIndex]) : this.placeholder || 'p-emptylabel';
        },
        editableInputValue() {
            const selectedOptionIndex = this.findSelectedOptionIndex();

            return selectedOptionIndex !== -1 ? this.getOptionLabel(this.visibleOptions[selectedOptionIndex]) : this.modelValue || '';
        },
        equalityKey() {
            return this.optionValue ? null : this.dataKey;
        },
        searchFields() {
            return this.filterFields || [this.optionLabel];
        },
        filterResultMessageText() {
            return ObjectUtils.isNotEmpty(this.visibleOptions) ? this.filterMessageText.replaceAll('{0}', this.visibleOptions.length) : this.emptyFilterMessageText;
        },
        filterMessageText() {
            return this.filterMessage || this.$primevue.config.locale.searchMessage || '';
        },
        emptyFilterMessageText() {
            return this.emptyFilterMessage || this.$primevue.config.locale.emptySearchMessage || this.$primevue.config.locale.emptyFilterMessage || '';
        },
        emptyMessageText() {
            return this.emptyMessage || this.$primevue.config.locale.emptyMessage || '';
        },
        selectionMessageText() {
            return this.selectionMessage || this.$primevue.config.locale.selectionMessage || '';
        },
        emptySelectionMessageText() {
            return this.emptySelectionMessage || this.$primevue.config.locale.emptySelectionMessage || '';
        },
        selectedMessageText() {
            return this.hasSelectedOption ? this.selectionMessageText.replaceAll('{0}', '1') : this.emptySelectionMessageText;
        },
        id() {
            return this.$attrs.id || UniqueComponentId();
        },
        focusedOptionId() {
            return this.focusedOptionIndex !== -1 ? `${this.id}_${this.focusedOptionIndex}` : null;
        },
        ariaSetSize() {
            return this.visibleOptions.filter((option) => !this.isOptionGroup(option)).length;
        },
        virtualScrollerDisabled() {
            return !this.virtualScrollerOptions;
        }
    },
    directives: {
        ripple: Ripple
    },
    components: {
        VirtualScroller: VirtualScroller,
        Portal: Portal
    }
};

const _hoisted_1 = ["id"];
const _hoisted_2 = ["id", "value", "placeholder", "tabindex", "disabled", "aria-label", "aria-labelledby", "aria-expanded", "aria-controls", "aria-activedescendant"];
const _hoisted_3 = ["id", "tabindex", "aria-label", "aria-labelledby", "aria-expanded", "aria-controls", "aria-activedescendant", "aria-disabled"];
const _hoisted_4 = { class: "p-dropdown-trigger" };
const _hoisted_5 = {
  key: 0,
  class: "p-dropdown-header"
};
const _hoisted_6 = { class: "p-dropdown-filter-container" };
const _hoisted_7 = ["value", "placeholder", "aria-owns", "aria-activedescendant"];
const _hoisted_8 = {
  role: "status",
  "aria-live": "polite",
  class: "p-hidden-accessible"
};
const _hoisted_9 = ["id"];
const _hoisted_10 = ["id"];
const _hoisted_11 = ["id", "aria-label", "aria-selected", "aria-disabled", "aria-setsize", "aria-posinset", "onClick", "onMousemove"];
const _hoisted_12 = {
  key: 0,
  class: "p-dropdown-empty-message",
  role: "option"
};
const _hoisted_13 = {
  key: 1,
  class: "p-dropdown-empty-message",
  role: "option"
};
const _hoisted_14 = {
  key: 1,
  role: "status",
  "aria-live": "polite",
  class: "p-hidden-accessible"
};
const _hoisted_15 = {
  role: "status",
  "aria-live": "polite",
  class: "p-hidden-accessible"
};

function render(_ctx, _cache, $props, $setup, $data, $options) {
  const _component_VirtualScroller = resolveComponent("VirtualScroller");
  const _component_Portal = resolveComponent("Portal");
  const _directive_ripple = resolveDirective("ripple");

  return (openBlock(), createElementBlock("div", {
    ref: "container",
    id: $options.id,
    class: normalizeClass($options.containerClass),
    onClick: _cache[16] || (_cache[16] = (...args) => ($options.onContainerClick && $options.onContainerClick(...args)))
  }, [
    ($props.editable)
      ? (openBlock(), createElementBlock("input", mergeProps({
          key: 0,
          ref: "focusInput",
          id: $props.inputId,
          type: "text",
          style: $props.inputStyle,
          class: $options.inputStyleClass,
          value: $options.editableInputValue,
          placeholder: $props.placeholder,
          tabindex: !$props.disabled ? $props.tabindex : -1,
          disabled: $props.disabled,
          autocomplete: "off",
          role: "combobox",
          "aria-label": _ctx.ariaLabel,
          "aria-labelledby": _ctx.ariaLabelledby,
          "aria-haspopup": "listbox",
          "aria-expanded": $data.overlayVisible,
          "aria-controls": $options.id + '_list',
          "aria-activedescendant": $data.focused ? $options.focusedOptionId : undefined,
          onFocus: _cache[0] || (_cache[0] = (...args) => ($options.onFocus && $options.onFocus(...args))),
          onBlur: _cache[1] || (_cache[1] = (...args) => ($options.onBlur && $options.onBlur(...args))),
          onKeydown: _cache[2] || (_cache[2] = (...args) => ($options.onKeyDown && $options.onKeyDown(...args))),
          onInput: _cache[3] || (_cache[3] = (...args) => ($options.onEditableInput && $options.onEditableInput(...args)))
        }, $props.inputProps), null, 16, _hoisted_2))
      : (openBlock(), createElementBlock("span", mergeProps({
          key: 1,
          ref: "focusInput",
          id: $props.inputId,
          style: $props.inputStyle,
          class: $options.inputStyleClass,
          tabindex: !$props.disabled ? $props.tabindex : -1,
          role: "combobox",
          "aria-label": _ctx.ariaLabel || ($options.label === 'p-emptylabel' ? undefined : $options.label),
          "aria-labelledby": _ctx.ariaLabelledby,
          "aria-haspopup": "listbox",
          "aria-expanded": $data.overlayVisible,
          "aria-controls": $options.id + '_list',
          "aria-activedescendant": $data.focused ? $options.focusedOptionId : undefined,
          "aria-disabled": $props.disabled,
          onFocus: _cache[4] || (_cache[4] = (...args) => ($options.onFocus && $options.onFocus(...args))),
          onBlur: _cache[5] || (_cache[5] = (...args) => ($options.onBlur && $options.onBlur(...args))),
          onKeydown: _cache[6] || (_cache[6] = (...args) => ($options.onKeyDown && $options.onKeyDown(...args)))
        }, $props.inputProps), [
          renderSlot(_ctx.$slots, "value", {
            value: $props.modelValue,
            placeholder: $props.placeholder
          }, () => [
            createTextVNode(toDisplayString($options.label === 'p-emptylabel' ? ' ' : $options.label || 'empty'), 1)
          ])
        ], 16, _hoisted_3)),
    ($props.showClear && $props.modelValue != null)
      ? (openBlock(), createElementBlock("i", mergeProps({
          key: 2,
          class: ['p-dropdown-clear-icon', $props.clearIcon],
          onClick: _cache[7] || (_cache[7] = (...args) => ($options.onClearClick && $options.onClearClick(...args)))
        }, $props.clearIconProps), null, 16))
      : createCommentVNode("", true),
    createElementVNode("div", _hoisted_4, [
      renderSlot(_ctx.$slots, "indicator", {}, () => [
        createElementVNode("span", {
          class: normalizeClass($options.dropdownIconClass),
          "aria-hidden": "true"
        }, null, 2)
      ])
    ]),
    createVNode(_component_Portal, { appendTo: $props.appendTo }, {
      default: withCtx(() => [
        createVNode(Transition, {
          name: "p-connected-overlay",
          onEnter: $options.onOverlayEnter,
          onAfterEnter: $options.onOverlayAfterEnter,
          onLeave: $options.onOverlayLeave,
          onAfterLeave: $options.onOverlayAfterLeave
        }, {
          default: withCtx(() => [
            ($data.overlayVisible)
              ? (openBlock(), createElementBlock("div", mergeProps({
                  key: 0,
                  ref: $options.overlayRef,
                  style: $props.panelStyle,
                  class: $options.panelStyleClass,
                  onClick: _cache[14] || (_cache[14] = (...args) => ($options.onOverlayClick && $options.onOverlayClick(...args))),
                  onKeydown: _cache[15] || (_cache[15] = (...args) => ($options.onOverlayKeyDown && $options.onOverlayKeyDown(...args)))
                }, $props.panelProps), [
                  createElementVNode("span", {
                    ref: "firstHiddenFocusableElementOnOverlay",
                    role: "presentation",
                    "aria-hidden": "true",
                    class: "p-hidden-accessible p-hidden-focusable",
                    tabindex: 0,
                    onFocus: _cache[8] || (_cache[8] = (...args) => ($options.onFirstHiddenFocus && $options.onFirstHiddenFocus(...args)))
                  }, null, 544),
                  renderSlot(_ctx.$slots, "header", {
                    value: $props.modelValue,
                    options: $options.visibleOptions
                  }),
                  ($props.filter)
                    ? (openBlock(), createElementBlock("div", _hoisted_5, [
                        createElementVNode("div", _hoisted_6, [
                          createElementVNode("input", mergeProps({
                            ref: "filterInput",
                            type: "text",
                            value: $data.filterValue,
                            onVnodeUpdated: _cache[9] || (_cache[9] = (...args) => ($options.onFilterUpdated && $options.onFilterUpdated(...args))),
                            class: "p-dropdown-filter p-inputtext p-component",
                            placeholder: $props.filterPlaceholder,
                            role: "searchbox",
                            autocomplete: "off",
                            "aria-owns": $options.id + '_list',
                            "aria-activedescendant": $options.focusedOptionId,
                            onKeydown: _cache[10] || (_cache[10] = (...args) => ($options.onFilterKeyDown && $options.onFilterKeyDown(...args))),
                            onBlur: _cache[11] || (_cache[11] = (...args) => ($options.onFilterBlur && $options.onFilterBlur(...args))),
                            onInput: _cache[12] || (_cache[12] = (...args) => ($options.onFilterChange && $options.onFilterChange(...args)))
                          }, $props.filterInputProps), null, 16, _hoisted_7),
                          createElementVNode("span", {
                            class: normalizeClass(['p-dropdown-filter-icon', $props.filterIcon])
                          }, null, 2)
                        ]),
                        createElementVNode("span", _hoisted_8, toDisplayString($options.filterResultMessageText), 1)
                      ]))
                    : createCommentVNode("", true),
                  createElementVNode("div", {
                    class: "p-dropdown-items-wrapper",
                    style: normalizeStyle({ 'max-height': $options.virtualScrollerDisabled ? $props.scrollHeight : '' })
                  }, [
                    createVNode(_component_VirtualScroller, mergeProps({ ref: $options.virtualScrollerRef }, $props.virtualScrollerOptions, {
                      items: $options.visibleOptions,
                      style: { height: $props.scrollHeight },
                      tabindex: -1,
                      disabled: $options.virtualScrollerDisabled
                    }), createSlots({
                      content: withCtx(({ styleClass, contentRef, items, getItemOptions, contentStyle, itemSize }) => [
                        createElementVNode("ul", {
                          ref: (el) => $options.listRef(el, contentRef),
                          id: $options.id + '_list',
                          class: normalizeClass(['p-dropdown-items', styleClass]),
                          style: normalizeStyle(contentStyle),
                          role: "listbox"
                        }, [
                          (openBlock(true), createElementBlock(Fragment, null, renderList(items, (option, i) => {
                            return (openBlock(), createElementBlock(Fragment, {
                              key: $options.getOptionRenderKey(option, $options.getOptionIndex(i, getItemOptions))
                            }, [
                              ($options.isOptionGroup(option))
                                ? (openBlock(), createElementBlock("li", {
                                    key: 0,
                                    id: $options.id + '_' + $options.getOptionIndex(i, getItemOptions),
                                    style: normalizeStyle({ height: itemSize ? itemSize + 'px' : undefined }),
                                    class: "p-dropdown-item-group",
                                    role: "option"
                                  }, [
                                    renderSlot(_ctx.$slots, "optiongroup", {
                                      option: option.optionGroup,
                                      index: $options.getOptionIndex(i, getItemOptions)
                                    }, () => [
                                      createTextVNode(toDisplayString($options.getOptionGroupLabel(option.optionGroup)), 1)
                                    ])
                                  ], 12, _hoisted_10))
                                : withDirectives((openBlock(), createElementBlock("li", {
                                    key: 1,
                                    id: $options.id + '_' + $options.getOptionIndex(i, getItemOptions),
                                    style: normalizeStyle({ height: itemSize ? itemSize + 'px' : undefined }),
                                    class: normalizeClass(['p-dropdown-item', { 'p-highlight': $options.isSelected(option), 'p-focus': $data.focusedOptionIndex === $options.getOptionIndex(i, getItemOptions), 'p-disabled': $options.isOptionDisabled(option) }]),
                                    role: "option",
                                    "aria-label": $options.getOptionLabel(option),
                                    "aria-selected": $options.isSelected(option),
                                    "aria-disabled": $options.isOptionDisabled(option),
                                    "aria-setsize": $options.ariaSetSize,
                                    "aria-posinset": $options.getAriaPosInset($options.getOptionIndex(i, getItemOptions)),
                                    onClick: $event => ($options.onOptionSelect($event, option)),
                                    onMousemove: $event => ($options.onOptionMouseMove($event, $options.getOptionIndex(i, getItemOptions)))
                                  }, [
                                    renderSlot(_ctx.$slots, "option", {
                                      option: option,
                                      index: $options.getOptionIndex(i, getItemOptions)
                                    }, () => [
                                      createTextVNode(toDisplayString($options.getOptionLabel(option)), 1)
                                    ])
                                  ], 46, _hoisted_11)), [
                                    [_directive_ripple]
                                  ])
                            ], 64))
                          }), 128)),
                          ($data.filterValue && (!items || (items && items.length === 0)))
                            ? (openBlock(), createElementBlock("li", _hoisted_12, [
                                renderSlot(_ctx.$slots, "emptyfilter", {}, () => [
                                  createTextVNode(toDisplayString($options.emptyFilterMessageText), 1)
                                ])
                              ]))
                            : (!$props.options || ($props.options && $props.options.length === 0))
                              ? (openBlock(), createElementBlock("li", _hoisted_13, [
                                  renderSlot(_ctx.$slots, "empty", {}, () => [
                                    createTextVNode(toDisplayString($options.emptyMessageText), 1)
                                  ])
                                ]))
                              : createCommentVNode("", true)
                        ], 14, _hoisted_9)
                      ]),
                      _: 2
                    }, [
                      (_ctx.$slots.loader)
                        ? {
                            name: "loader",
                            fn: withCtx(({ options }) => [
                              renderSlot(_ctx.$slots, "loader", { options: options })
                            ]),
                            key: "0"
                          }
                        : undefined
                    ]), 1040, ["items", "style", "disabled"])
                  ], 4),
                  renderSlot(_ctx.$slots, "footer", {
                    value: $props.modelValue,
                    options: $options.visibleOptions
                  }),
                  (!$props.options || ($props.options && $props.options.length === 0))
                    ? (openBlock(), createElementBlock("span", _hoisted_14, toDisplayString($options.emptyMessageText), 1))
                    : createCommentVNode("", true),
                  createElementVNode("span", _hoisted_15, toDisplayString($options.selectedMessageText), 1),
                  createElementVNode("span", {
                    ref: "lastHiddenFocusableElementOnOverlay",
                    role: "presentation",
                    "aria-hidden": "true",
                    class: "p-hidden-accessible p-hidden-focusable",
                    tabindex: 0,
                    onFocus: _cache[13] || (_cache[13] = (...args) => ($options.onLastHiddenFocus && $options.onLastHiddenFocus(...args)))
                  }, null, 544)
                ], 16))
              : createCommentVNode("", true)
          ]),
          _: 3
        }, 8, ["onEnter", "onAfterEnter", "onLeave", "onAfterLeave"])
      ]),
      _: 3
    }, 8, ["appendTo"])
  ], 10, _hoisted_1))
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

var css_248z = "\n.p-dropdown {\n    display: -webkit-inline-box;\n    display: -ms-inline-flexbox;\n    display: inline-flex;\n    cursor: pointer;\n    position: relative;\n    -webkit-user-select: none;\n       -moz-user-select: none;\n        -ms-user-select: none;\n            user-select: none;\n}\n.p-dropdown-clear-icon {\n    position: absolute;\n    top: 50%;\n    margin-top: -0.5rem;\n}\n.p-dropdown-trigger {\n    display: -webkit-box;\n    display: -ms-flexbox;\n    display: flex;\n    -webkit-box-align: center;\n        -ms-flex-align: center;\n            align-items: center;\n    -webkit-box-pack: center;\n        -ms-flex-pack: center;\n            justify-content: center;\n    -ms-flex-negative: 0;\n        flex-shrink: 0;\n}\n.p-dropdown-label {\n    display: block;\n    white-space: nowrap;\n    overflow: hidden;\n    -webkit-box-flex: 1;\n        -ms-flex: 1 1 auto;\n            flex: 1 1 auto;\n    width: 1%;\n    text-overflow: ellipsis;\n    cursor: pointer;\n}\n.p-dropdown-label-empty {\n    overflow: hidden;\n    opacity: 0;\n}\ninput.p-dropdown-label {\n    cursor: default;\n}\n.p-dropdown .p-dropdown-panel {\n    min-width: 100%;\n}\n.p-dropdown-panel {\n    position: absolute;\n    top: 0;\n    left: 0;\n}\n.p-dropdown-items-wrapper {\n    overflow: auto;\n}\n.p-dropdown-item {\n    cursor: pointer;\n    font-weight: normal;\n    white-space: nowrap;\n    position: relative;\n    overflow: hidden;\n}\n.p-dropdown-item-group {\n    cursor: auto;\n}\n.p-dropdown-items {\n    margin: 0;\n    padding: 0;\n    list-style-type: none;\n}\n.p-dropdown-filter {\n    width: 100%;\n}\n.p-dropdown-filter-container {\n    position: relative;\n}\n.p-dropdown-filter-icon {\n    position: absolute;\n    top: 50%;\n    margin-top: -0.5rem;\n}\n.p-fluid .p-dropdown {\n    display: -webkit-box;\n    display: -ms-flexbox;\n    display: flex;\n}\n.p-fluid .p-dropdown .p-dropdown-label {\n    width: 1%;\n}\n";
styleInject(css_248z);

script.render = render;

export { script as default };
