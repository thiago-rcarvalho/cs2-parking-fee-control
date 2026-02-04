declare module 'cs2/modding' {
  export interface ModuleRegistry {
    append(target: string, component: React.ComponentType<any>): void;
    extend(id: string, component: React.ComponentType<any>): void;
  }

  export type ModRegistrar = (moduleRegistry: ModuleRegistry) => void;
}

declare module 'cs2/api' {
  export function bindValue<T>(
    namespace: string,
    key: string,
    defaultValue?: T
  ): { subscribe(callback: (value: T) => void): () => void };

  export function useValue<T>(binding: { subscribe(callback: (value: T) => void): () => void }): T;

  export function trigger(namespace: string, eventName: string, ...args: any[]): void;
}

declare module 'cs2/bindings' {
  // Bindings module types
}

declare module 'cs2/ui' {
  import React from 'react';

  export const FOCUS_AUTO: unique symbol;
  export type FocusKey = typeof FOCUS_AUTO | string | number;

  export interface PanelProps {
    header?: string;
    children?: React.ReactNode;
    onClose?: () => void;
    className?: string;
    focusKey?: FocusKey;
    allowFocusExit?: boolean;
  }

  export const Panel: React.FC<PanelProps>;

  export interface ButtonProps {
    children?: React.ReactNode;
    onSelect?: () => void;
    className?: string;
    disabled?: boolean;
  }

  export const Button: React.FC<ButtonProps>;

  export interface TooltipProps {
    tooltip?: string;
    children?: React.ReactNode;
  }

  export const Tooltip: React.FC<TooltipProps>;
}

declare module 'cs2/l10n' {
  export interface LocalizationContext {
    translate(key: string, fallback?: string): string | undefined;
  }

  export function useLocalization(): LocalizationContext;
}

declare module 'cs2/input' {
  import React from 'react';

  export type InputAction = string;

  export interface SingleActionConsumerProps {
    action?: InputAction;
    actionContext?: string;
    disabled?: boolean;
    onAction?: () => void;
  }

  export interface InputActionConsumerAction {
    actionContext?: string;
    onAction?: () => void;
  }

  export type InputActionConsumerActions = Record<InputAction, (() => void) | InputActionConsumerAction | null | undefined>;

  export interface InputActionConsumerProps {
    actions?: InputActionConsumerActions;
    actionContext?: string;
    disabled?: boolean;
    ignoreFocusState?: boolean;
  }

  export const InputActionConsumer: React.FC<React.PropsWithChildren<InputActionConsumerProps>>;

  /** When the Keyboard "ESC" or Gamepad "B" button is pressed */
  export const BackConsumer: React.FC<React.PropsWithChildren<SingleActionConsumerProps>>;
  export const CloseConsumer: React.FC<React.PropsWithChildren<SingleActionConsumerProps>>;
  export const SelectConsumer: React.FC<React.PropsWithChildren<SingleActionConsumerProps>>;
  export const ExpandConsumer: React.FC<React.PropsWithChildren<SingleActionConsumerProps>>;

  export interface InputActionBarrierProps {
    includes?: InputAction[];
    excludes?: InputAction[];
    disabled?: boolean;
  }

  export const InputActionBarrier: React.FC<React.PropsWithChildren<InputActionBarrierProps>>;
}
