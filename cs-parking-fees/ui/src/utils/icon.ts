export const getIconPath = (name?: string): string => {
  const key = name && name.trim() !== '' ? name : 'Parking';
  return `Media/Game/Icons/${key}.svg`;
};

export default getIconPath;
