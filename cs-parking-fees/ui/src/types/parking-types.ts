export interface ParkingFeeConfig {
  enabled: boolean;
  defaultParkingFee: number;
  categories: Category[];
}

export interface Category {
  type: string;
  defaultFee: number;
  icon?: string;
  prefabs: Prefab[];
}

export interface Prefab {
  name: string;
  displayName: string;
  thumbnail?: string;
  fee: number;
}

export interface CategoryFeeUpdate {
  categoryType: string;
  newFee: number;
}

export interface PrefabFeeUpdate {
  categoryType: string;
  prefabName: string;
  newFee: number;
}
