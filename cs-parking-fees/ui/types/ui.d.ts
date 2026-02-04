declare module "cs2/ui" {
  import { CSSProperties, HTMLAttributes, PropsWithChildren, ReactElement, ReactNode, Ref } from 'react';

  export interface RefReactElement<T = any, P = any> extends ReactElement<P> {
    ref?: Ref<T>;
  }

  export interface ClassProps {
    className?: string;
  }

  export type BalloonDirection = "up" | "down" | "left" | "right";
  export type BalloonAlignment = "start" | "center" | "end";

  export interface TooltipProps extends ClassProps {
    tooltip?: ReactNode;
    forceVisible?: boolean;
    disabled?: boolean;
    theme?: Partial<any>;
    direction?: BalloonDirection;
    alignment?: BalloonAlignment;
  }
  
  export const Tooltip: (props: PropsWithChildren<TooltipProps>) => JSX.Element;

  export const FOCUS_DISABLED: unique symbol;
  export const FOCUS_AUTO: unique symbol;
  export type FocusKey = typeof FOCUS_DISABLED | typeof FOCUS_AUTO | string | number;

  export enum UISound {
    selectItem = "select-item",
    dragSlider = "drag-slider",
    hoverItem = "hover-item",
    expandPanel = "expand-panel",
    grabSlider = "grabSlider",
    selectDropdown = "select-dropdown",
    selectToggle = "select-toggle",
    focusInputField = "focus-input-field",
  }

  export type Action = () => void | boolean;
  export type Action1D = (value: number) => void | boolean;

  export type InputAction = string;

  export interface ButtonTheme {
    button: string;
  }

  export interface ButtonSounds {
    select?: UISound | string | null;
    hover?: UISound | string | null;
    focus?: UISound | string | null;
  }

  export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement | HTMLDivElement> {
    focusKey?: FocusKey;
    debugName?: string;
    selected?: boolean;
    theme?: Partial<ButtonTheme>;
    sounds?: ButtonSounds | null;
    selectAction?: InputAction;
    selectSound?: UISound | string | null;
    tooltipLabel?: ReactNode;
    onSelect?: () => void;
    as?: "button" | "div";
  }

  export interface IconButtonTheme extends ButtonTheme {
    icon: string;
  }

  export interface IconButtonProps extends ButtonProps {
    src: string;
    tinted?: boolean;
    theme?: Partial<IconButtonTheme>;
  }

  export interface LabeledIconButtonTheme extends IconButtonTheme {
    label: string;
  }

  export interface LabeledIconButtonProps extends IconButtonProps {
    label?: string;
  }

  type ButtonPropsVariant = ButtonProps & Partial<LabeledIconButtonProps> & {
    variant?: "flat" | "primary" | "round" | "menu" | "icon" | "floating" | "default";
    theme?: Partial<LabeledIconButtonTheme>;
  };

  export const Button: (props: ButtonPropsVariant) => JSX.Element;
  export const IconButton: (props: Partial<IconButtonProps>) => JSX.Element;
  export const MenuButton: (props: Partial<LabeledIconButtonProps>) => JSX.Element;
  export const FloatingButton: (props: Partial<IconButtonProps>) => JSX.Element;

  export interface Number2 {
    readonly x: number;
    readonly y: number;
  }

  export interface TransitionStyles {
    enter?: string;
    enterActive?: string;
    exit?: string;
    exitActive?: string;
  }

  export interface TransitionSounds {
    enter?: UISound | string | null;
    exit?: UISound | string | null;
  }

  export interface PanelTheme {
    panel: string;
    header: string;
    content: string;
    footer: string;
    titleBar: string;
    title: string;
    icon: string;
    iconSpace: string;
    closeButton: string;
    closeIcon: string;
    toggle: string;
    toggleIcon: string;
    toggleIconExpanded: string;
  }

  export interface PanelProps extends HTMLAttributes<HTMLDivElement> {
    focusKey?: FocusKey;
    header?: ReactNode;
    footer?: ReactNode;
    theme?: Partial<PanelTheme>;
    transition?: TransitionStyles | null;
    transitionSounds?: TransitionSounds | null;
    contentClassName?: string;
    onClose?: () => void;
    allowFocusExit?: boolean;
  }

  export interface DraggablePanelProps extends PanelProps {
    initialPosition?: Number2;
    draggable: true;
  }

  export interface SimplePanelProps extends PanelProps {
    draggable?: false | undefined;
  }

  type PanelPropsType = SimplePanelProps | DraggablePanelProps;

  export const Panel: (props: PropsWithChildren<PanelPropsType>) => JSX.Element;

  export interface IconProps {
    src: string;
    tinted?: boolean;
    className?: string;
  }

  export const Icon: (props: IconProps) => JSX.Element;

  export interface ScrollableProps {
    smooth?: boolean;
    horizontal?: boolean;
    vertical?: boolean;
    trackVisibility?: "always" | "scrolling" | "scrollable";
    style?: CSSProperties;
    contentStyle?: CSSProperties;
    trackStyle?: CSSProperties;
    className?: string;
    contentClassName?: string;
  }

  export const Scrollable: (props: ScrollableProps & { children?: ReactNode }) => JSX.Element;

  export interface InfoSectionProps extends ClassProps {
    focusKey?: FocusKey;
    tooltip?: ReactNode;
    disableFocus?: boolean;
  }

  export const InfoSection: (props: PropsWithChildren<InfoSectionProps>) => JSX.Element;

  export interface InfoRowProps extends ClassProps {
    icon?: string;
    left?: ReactNode;
    right?: ReactNode;
    tooltip?: ReactNode;
    link?: ReactNode;
    uppercase?: boolean;
    subRow?: boolean;
    disableFocus?: boolean;
  }

  export const InfoRow: (props: InfoRowProps) => JSX.Element;

  export const Portal: {
    ({ children }: PropsWithChildren<{}>): React.ReactPortal;
    usePortalContainer: () => HTMLElement;
    ContainerProvider: ({ children }: { children: RefReactElement<HTMLElement> }) => JSX.Element;
  };
}
