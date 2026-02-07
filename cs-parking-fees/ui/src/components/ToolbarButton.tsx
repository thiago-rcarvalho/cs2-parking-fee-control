import React from 'react';

// Custom floating button styled like the game's native buttons
// Using longhand CSS properties as cohtml doesn't support shorthand with variables
const buttonStyle: React.CSSProperties = {
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  width: 'var(--floatingToggleSize)',
  height: 'var(--floatingToggleSize)',
  paddingTop: 'var(--gap2)',
  paddingRight: 'var(--gap2)',
  paddingBottom: 'var(--gap2)',
  paddingLeft: 'var(--gap2)',
  backgroundColor: 'var(--accentColorNormal)',
  borderTopLeftRadius: 'var(--floatingToggleBorderRadius)',
  borderTopRightRadius: 'var(--floatingToggleBorderRadius)',
  borderBottomLeftRadius: 'var(--floatingToggleBorderRadius)',
  borderBottomRightRadius: 'var(--floatingToggleBorderRadius)',
  marginRight: '6rem',
  marginBottom: '6rem',
  cursor: 'pointer',
  border: 'none',
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
  const [hovered, setHovered] = React.useState(false);

  const handleClick = () => {
    //console.log('[ParkingFeeControl] Button clicked!');
    onClick();
  };

  const currentStyle: React.CSSProperties = {
    ...buttonStyle,
    backgroundColor: hovered ? 'var(--accentColorNormal-hover)' : 'var(--accentColorNormal)',
  };

  return (
    <div
      style={currentStyle}
      onClick={handleClick}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      title="Parking Fee Configuration"
    >
      <ParkingIcon />
    </div>
  );
};
