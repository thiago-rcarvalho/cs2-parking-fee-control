import { FocusKey, Theme, UniqueFocusKey } from "cs2/bindings";
import { ModuleRegistry, getModule } from "cs2/modding";
import { CSSProperties, ReactNode } from "react";

// Slider props based on the game's slider component
type SliderProps = {
  focusKey?: FocusKey;
  value: number;
  start: number;
  end: number;
  gamepadStep?: number;
  disabled?: boolean;
  vertical?: boolean;
  sounds?: boolean;
  thumb?: any;
  theme?: Theme | any;
  className?: string;
  style?: CSSProperties;
  children?: ReactNode;
  noFill?: boolean;
  valueTransformer?: (e: number, t: number, n: number) => number;
  onChange?: (value: number) => void;
  onDragStart?: () => void;
  onDragEnd?: () => void;
  onMouseOver?: () => void;
  onMouseLeave?: () => void;
};

type PropsToolButton = {
  focusKey?: UniqueFocusKey | null;
  src: string;
  selected?: boolean;
  multiSelect?: boolean;
  disabled?: boolean;
  tooltip?: string | null;
  selectSound?: any;
  uiTag?: string;
  className?: string;
  children?: string | JSX.Element | JSX.Element[];
  onSelect?: (x: any) => any;
};

type PropsSection = {
  title?: string | null;
  uiTag?: string;
  children: string | JSX.Element | JSX.Element[];
};

// Registry index for game components - using getModule instead
const registryPaths = {
  Section: "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
  ToolButton: "game-ui/game/components/tool-options/tool-button/tool-button.tsx",
  toolButtonTheme: "game-ui/game/components/tool-options/tool-button/tool-button.module.scss",
  mouseToolOptionsTheme: "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.module.scss",
  FOCUS_DISABLED: "game-ui/common/focus/focus-key.ts",
  FOCUS_AUTO: "game-ui/common/focus/focus-key.ts",
  useUniqueFocusKey: "game-ui/common/focus/focus-key.ts",
  Slider: "game-ui/common/input/slider/slider.tsx",
  sliderTheme: "game-ui/common/input/slider/slider.module.scss",
};

export class VanillaComponentResolver {
  public static get instance(): VanillaComponentResolver {
    if (!this._instance) {
      this._instance = new VanillaComponentResolver();
    }
    return this._instance;
  }
  private static _instance?: VanillaComponentResolver;

  public static setRegistry(in_registry: ModuleRegistry) {
    // Registry is set but we use getModule for access
    this._instance = new VanillaComponentResolver();
  }

  private cachedData: Record<string, any> = {};

  public get Section(): (props: PropsSection) => JSX.Element {
    if (!this.cachedData["Section"]) {
      this.cachedData["Section"] = getModule(registryPaths.Section, "Section");
    }
    return this.cachedData["Section"];
  }
  
  public get ToolButton(): (props: PropsToolButton) => JSX.Element {
    if (!this.cachedData["ToolButton"]) {
      this.cachedData["ToolButton"] = getModule(registryPaths.ToolButton, "ToolButton");
    }
    return this.cachedData["ToolButton"];
  }
  
  public get Slider(): (props: SliderProps) => JSX.Element {
    if (!this.cachedData["Slider"]) {
      this.cachedData["Slider"] = getModule(registryPaths.Slider, "Slider");
    }
    return this.cachedData["Slider"];
  }

  public get toolButtonTheme(): Theme | any {
    if (!this.cachedData["toolButtonTheme"]) {
      this.cachedData["toolButtonTheme"] = getModule(registryPaths.toolButtonTheme, "classes");
    }
    return this.cachedData["toolButtonTheme"];
  }
  
  public get mouseToolOptionsTheme(): Theme | any {
    if (!this.cachedData["mouseToolOptionsTheme"]) {
      this.cachedData["mouseToolOptionsTheme"] = getModule(registryPaths.mouseToolOptionsTheme, "classes");
    }
    return this.cachedData["mouseToolOptionsTheme"];
  }
  
  public get sliderTheme(): Theme | any {
    if (!this.cachedData["sliderTheme"]) {
      this.cachedData["sliderTheme"] = getModule(registryPaths.sliderTheme, "classes");
    }
    return this.cachedData["sliderTheme"];
  }

  public get FOCUS_DISABLED(): UniqueFocusKey {
    if (!this.cachedData["FOCUS_DISABLED"]) {
      this.cachedData["FOCUS_DISABLED"] = getModule(registryPaths.FOCUS_DISABLED, "FOCUS_DISABLED");
    }
    return this.cachedData["FOCUS_DISABLED"];
  }
  
  public get FOCUS_AUTO(): UniqueFocusKey {
    if (!this.cachedData["FOCUS_AUTO"]) {
      this.cachedData["FOCUS_AUTO"] = getModule(registryPaths.FOCUS_AUTO, "FOCUS_AUTO");
    }
    return this.cachedData["FOCUS_AUTO"];
  }
  
  public get useUniqueFocusKey(): (focusKey: FocusKey, debugName: string) => UniqueFocusKey | null {
    if (!this.cachedData["useUniqueFocusKey"]) {
      this.cachedData["useUniqueFocusKey"] = getModule(registryPaths.useUniqueFocusKey, "useUniqueFocusKey");
    }
    return this.cachedData["useUniqueFocusKey"];
  }
}
