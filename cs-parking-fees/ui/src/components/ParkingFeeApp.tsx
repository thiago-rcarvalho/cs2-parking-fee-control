import React, { useState, useCallback } from 'react';
import { ToolbarButton } from './ToolbarButton';
import { ParkingFeePanel } from './ParkingFeePanel';
import { InputActionConsumer } from 'cs2/input';

export const ParkingFeeApp: React.FC = () => {
  const [showPanel, setShowPanel] = useState(false);

  const handleToggle = useCallback(() => {
    setShowPanel((prev) => !prev);
  }, []);

  return (
    <>
      <ToolbarButton onClick={handleToggle} />
      {showPanel && (
        <InputActionConsumer
          ignoreFocusState
          actions={{
            Back: handleToggle,
            "Pause Menu": handleToggle,
          }}
        >
          <ParkingFeePanel onClose={handleToggle} />
        </InputActionConsumer>
      )}
    </>
  );
};
