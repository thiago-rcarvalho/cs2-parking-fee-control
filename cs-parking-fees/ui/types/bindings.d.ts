declare module "cs2/bindings" {
  export interface Entity {
    index: number;
    version: number;
  }

  export interface Color {
    r: number;
    g: number;
    b: number;
    a: number;
  }

  export interface Theme {
    entity: Entity;
    name: string;
    icon: string;
  }

  export type BalloonDirection = "up" | "down" | "left" | "right";
  
  export type FocusKey = string | number | symbol;
  
  export interface UniqueFocusKey {
    readonly key: string;
    readonly debugName: string;
  }

  export namespace game {
    const activeGamePanel$: import("cs2/api").ValueBinding<{ __Type: GamePanelType }>;
    enum GamePanelType {
      None = "None",
      PhotoMode = "PhotoMode",
    }
  }
}
