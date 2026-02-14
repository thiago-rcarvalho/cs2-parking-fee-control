import React from 'react';
import { getModule } from 'cs2/modding';

// Custom floating button styled like the game's native buttons
// Using longhand CSS properties as cohtml doesn't support shorthand with variables
const buttonStyle: React.CSSProperties = {
  marginRight: '6rem',
  marginBottom: '6rem',
};

const iconStyle: React.CSSProperties = {
  width: '100%',
  height: '100%',
};

// Parking icon using the game's default asset
const ParkingIcon: React.FC = () => (
  <img
    src="Media/Game/Icons/Parking.svg"
    alt="Parking Icon"
    style={iconStyle}
    draggable={false}
  />
);

interface ToolbarButtonProps {
  onClick: () => void;
}

export const ToolbarButton: React.FC<ToolbarButtonProps> = ({ onClick }) => {
  const floatingButtonClasses = getModule(
    'game-ui/common/input/button/floating-icon-button.module.scss',
    'classes'
  ) as Record<string, string> | undefined;

  const handleClick = () => {
    //console.log('[ParkingFeeControl] Button clicked!');
    onClick();
  };

  return (
    <button
      type="button"
      className={floatingButtonClasses?.button}
      style={buttonStyle}
      onClick={handleClick}
      title="Parking Fee Configuration"
    >
      <ParkingIcon />
    </button>
  );
};
