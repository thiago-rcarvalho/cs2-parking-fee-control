import React, { useState, useRef, useCallback, useEffect } from 'react';
import { bindValue, trigger, useValue } from 'cs2/api';
import { Scrollable, Panel } from 'cs2/ui';
import { useLocalization } from 'cs2/l10n';
import { ParkingFeeConfig, CategoryFeeUpdate, PrefabFeeUpdate } from '../types/parking-types';

const config$ = bindValue<ParkingFeeConfig>('parkingfee', 'config');

interface ParkingFeePanelProps {
  onClose: () => void;
}

// ============== CUSTOM SLIDER COMPONENT ==============
// Based EXACTLY on game's slider HTML/CSS structure
interface CustomSliderProps {
  value: number;
  min: number;
  max: number;
  onChange: (value: number) => void;
}

const sliderStyles = {
  // Game: wrapper_TUT + slider_lZg
  wrapper: {
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
    flex: '1',
  },
  // Game: slider_pUS horizontal
  slider: {
    flex: '1',
    position: 'relative' as const,
    height: '8rem',
    backgroundColor: '#999',
    borderRadius: '4rem',
    cursor: 'pointer',
  },
  // Game: track-bounds_H8_
  trackBounds: {
    width: '100%',
    height: '100%',
    display: 'flex',
    flexDirection: 'row' as const,
  },
  // Game: range-bounds_lNt (width based on value)
  rangeBounds: {
    position: 'relative' as const,
    height: '100%',
  },
  // Game: range_K3G (the filled part)
  range: {
    position: 'absolute' as const,
    top: '0',
    bottom: '0',
    left: '0',
    right: '0',
    backgroundColor: 'var(--accentColorNormal)',
    borderRadius: '4rem',
  },
  // Game: thumb-container_aso
  thumbContainer: {
    position: 'absolute' as const,
    top: '0',
    right: '0',
    width: '0',
    height: '100%',
    display: 'flex',
    flexDirection: 'column' as const,
    justifyContent: 'center',
    alignItems: 'center',
  },
  // Game: thumb_WZt
  thumb: {
    width: '16rem',
    height: '16rem',
    borderRadius: '50%',
    backgroundColor: '#ddd',
    cursor: 'grab',
  },
};

const CustomSlider: React.FC<CustomSliderProps> = ({ value, min, max, onChange }) => {
  const sliderRef = useRef<HTMLDivElement>(null);
  const [isDragging, setIsDragging] = useState(false);

  const calculateValue = useCallback((clientX: number) => {
    if (!sliderRef.current) return value;
    const rect = sliderRef.current.getBoundingClientRect();
    const percentage = Math.max(0, Math.min(1, (clientX - rect.left) / rect.width));
    return Math.round(min + percentage * (max - min));
  }, [min, max, value]);

  const handleMouseDown = (e: React.MouseEvent) => {
    e.preventDefault();
    setIsDragging(true);
    const newValue = calculateValue(e.clientX);
    onChange(newValue);

    const handleMouseMove = (moveEvent: MouseEvent) => {
      const newVal = calculateValue(moveEvent.clientX);
      onChange(newVal);
    };

    const handleMouseUp = () => {
      setIsDragging(false);
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };

    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);
  };

  const percentage = ((value - min) / (max - min)) * 100;

  return (
    <div style={sliderStyles.wrapper}>
      <div 
        ref={sliderRef}
        style={sliderStyles.slider}
        onMouseDown={handleMouseDown}
      >
        <div style={sliderStyles.trackBounds}>
          <div style={{ ...sliderStyles.rangeBounds, width: `${percentage}%` }}>
            <div style={sliderStyles.range} />
            <div style={sliderStyles.thumbContainer}>
              <div style={{
                ...sliderStyles.thumb,
                cursor: isDragging ? 'grabbing' : 'grab',
              }} />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

const useRefreshOnOpen = () => {
  useEffect(() => {
    trigger('parkingfee', 'refreshConfig');
  }, []);
};

// ============== INLINE STYLES ==============
// Based on game's Economy > Taxation panel structure
// Layout: [Type Column] | [Fee Rate Column] | [Estimate Column]

const S = {
  // Main panel - matches game economy panel (1000rem x 680rem fixed)
  container: {
    width: '1000rem',
    height: '680rem',
    backgroundColor: 'var(--panelColorNormal)',
    backdropFilter: 'var(--panelBlur)',
    borderRadius: '4rem',
    overflow: 'hidden',
    display: 'flex',
    flexDirection: 'column' as const,
    pointerEvents: 'auto' as const,
    boxShadow: '0 8rem 32rem rgba(0, 0, 0, 0.5)',
  },
  
  // Panel header
  // Column headers row - Game: table-header_qcq
  columnHeaders: {
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
    backgroundColor: 'var(--sectionHeaderColor)',
  },
  
  // Game: header-cell_ori + name_DfR {flex:1}
  columnType: {
    flex: '1',
    padding: '8rem 16rem',
    color: 'var(--textColorDim)',
    fontSize: 'var(--fontSizeXS)',
    fontWeight: '500',
    textTransform: 'uppercase' as const,
    whiteSpace: 'nowrap' as const,
  },
  
  // Game: header-cell_ori + rate_U86 {width:414rem;border-left}
  columnFeeRate: {
    width: '414rem',
    padding: '8rem 16rem',
    color: 'var(--textColorDim)',
    fontSize: 'var(--fontSizeXS)',
    fontWeight: '500',
    textTransform: 'uppercase' as const,
    textAlign: 'center' as const,
    borderLeftWidth: '2rem',
    borderLeftStyle: 'solid' as const,
    borderLeftColor: 'rgba(255,255,255,0.1)',
  },
  
  // Game: header-cell_ori + estimate_d79 {width:166rem;border-left}
  columnEstimate: {
    width: '166rem',
    padding: '8rem 16rem',
    color: 'var(--textColorDim)',
    fontSize: 'var(--fontSizeXS)',
    fontWeight: '500',
    textTransform: 'uppercase' as const,
    textAlign: 'center' as const,
    borderLeftWidth: '2rem',
    borderLeftStyle: 'solid' as const,
    borderLeftColor: 'rgba(255,255,255,0.1)',
  },
  
  // Content area with padding (Game: content_AD7)
  content: {
    flex: '1',
    padding: '10rem',
    overflow: 'hidden',
  },
  
  // Table section wrapper (Game: section_sop table-section_D_X)
  tableSection: {
    height: '100%',
    display: 'flex',
    flexDirection: 'column' as const,
    border: '1rem solid rgba(56,72,104,0.7)',
    borderRadius: '4rem',
    overflow: 'hidden',
  },
  
  // Table header wrapper (Game: header_l0j)
  tableHeader: {
    backgroundColor: 'var(--sectionHeaderColor)',
  },
  
  // Table content wrapper (Game: content_flM content_owQ)
  // This is a flex container that holds both scrollable and table-lines as siblings
  tableContentWrapper: {
    flex: '1',
    backgroundColor: 'rgba(38,49,71,0.5)',
    overflow: 'hidden',
    display: 'flex',
    flexDirection: 'column' as const,
  },

  // Table content scrollable area (Game: scrollable_Xqt)
  // flex: 0 1 auto - takes only the space needed for content, can shrink
  tableContent: {
    flex: '0 1 auto',
    overflow: 'hidden',
    // Scrollbar padding is set to 0 to make scrollbar overlay the content
    '--scrollbarPadding': '0rem',
  } as React.CSSProperties,

  // Table lines (Game: table-lines_Abv)
  // flex: 1 0 0 - fills the remaining space below the scrollable content
  // This draws the bottom border line and vertical column lines
  tableLines: {
    flex: '1 0 0',
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'stretch',
    pointerEvents: 'none' as const,
    overflow: 'hidden',
  },

  // Game: .table-lines_Abv div { border-top }
  // Name column in table-lines (Game: .name_DfR)
  tableLinesName: {
    flex: '1',
    borderTopStyle: 'solid' as const,
    borderTopWidth: '2rem',
    borderTopColor: 'rgba(255,255,255,0.1)',
  },

  // Rate column in table-lines (Game: .rate_U86)
  tableLinesRate: {
    width: '414rem',
    borderLeftStyle: 'solid' as const,
    borderLeftWidth: '2rem',
    borderLeftColor: 'rgba(255,255,255,0.1)',
    borderTopStyle: 'solid' as const,
    borderTopWidth: '2rem',
    borderTopColor: 'rgba(255,255,255,0.1)',
  },

  // Estimate column in table-lines (Game: .estimate_d79)
  tableLinesEstimate: {
    width: '166rem',
    borderLeftStyle: 'solid' as const,
    borderLeftWidth: '2rem',
    borderLeftColor: 'rgba(255,255,255,0.1)',
    borderTopStyle: 'solid' as const,
    borderTopWidth: '2rem',
    borderTopColor: 'rgba(255,255,255,0.1)',
  },
  
  // Category header row (Game: header_cPd)
  categoryHeader: {
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
    cursor: 'pointer',
    transition: 'background-color 0.15s ease',
    minHeight: '48rem',
  },
  
  categoryHeaderHover: {
    backgroundColor: 'rgba(255, 255, 255, 0.03)',
  },

  // Category group base (Game: taxation-group_aQb)
  categoryGroupBase: {
    // base styles - no border
  },

  // Category group with border-top (Game: taxation-group_aQb+.taxation-group_aQb)
  // The border goes on the GROUP container, not the header
  categoryGroupWithBorder: {
    borderTopStyle: 'solid' as const,
    borderTopWidth: '2rem',
    borderTopColor: 'rgba(255,255,255,0.1)',
  },
  
  // Game: .icon_vPX{width:48rem;height:48rem;margin:4rem 12rem 4rem 16rem}
  categoryIcon: {
    width: '48rem',
    height: '48rem',
    margin: '4rem 12rem 4rem 16rem',
    flexShrink: 0,
  },
  
  // Game: .title_WQ0{flex:1;overflow:hidden;text-align:left;font-size:var(--fontSizeL);color:var(--textColor)}
  categoryTitle: {
    flex: '1',
    overflow: 'visible',
    textAlign: 'left' as const,
    color: 'var(--textColor)',
    fontSize: 'var(--fontSizeL)',
    textTransform: 'capitalize' as const,
    whiteSpace: 'nowrap' as const,
  },
  
  // Expand button (Game: button_bvQ + toggle-button_hTU)
  expandButton: {
    width: '32rem',
    height: '32rem',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    flexShrink: 0,
    marginLeft: '12rem',
    marginRight: '12rem',
    backgroundColor: 'rgba(255,255,255,0)',
    borderRadius: '50%',
    border: 'none',
    cursor: 'pointer',
    transition: 'background-color 0.15s ease',
  },
  
  expandButtonHover: {
    backgroundColor: 'rgba(255,255,255,0.2)',
  },
  
  // Game: tinted-icon_iKo + icon_PhD
  expandIcon: {
    width: '100%',
    height: '100%',
    backgroundColor: 'var(--textColor)',
    maskImage: 'url(Media/Glyphs/ThickStrokeArrowDown.svg)',
    maskSize: 'contain',
    maskPosition: 'center',
    maskRepeat: 'no-repeat',
    WebkitMaskImage: 'url(Media/Glyphs/ThickStrokeArrowDown.svg)',
    WebkitMaskSize: 'contain',
    WebkitMaskPosition: 'center',
    WebkitMaskRepeat: 'no-repeat',
    transition: 'transform 0.2s ease',
  },
  
  expandIconExpanded: {
    transform: 'rotate(180deg)',
  },
  
  // Game: .tax-slider_HNg + .header-slider_wLI{width:580rem;align-self:stretch}
  taxSliderContainer: {
    width: '580rem',
    alignSelf: 'stretch',
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'stretch',
  },
  
  // Game: .rate_lAq - HIDDEN (percentual removido conforme solicitado)
  rateValue: {
    display: 'none',
  },
  
  // Game: .slider-column_XmW{flex:1;padding:4rem 22rem 4rem 4rem;display:flex;flex-direction:row;align-items:center;border-left:var(--stroke2) solid var(--dividerColor)}
  sliderColumn: {
    flex: '1',
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
    borderLeftWidth: '2rem',
    borderLeftStyle: 'solid' as const,
    borderLeftColor: 'rgba(255,255,255,0.1)',
    padding: '4rem 22rem 4rem 12rem',
  },
  
  // Game: wrapper_TUT + slider_lZg (wrapper for slider with flex:1)
  sliderWrapper: {
    flex: '1',
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
  },
  
  // Game: .estimate-column_DQB - wrapper for estimate value
  estimateColumn: {
    width: '166rem',
    padding: '6rem 12rem',
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
    borderLeftWidth: '2rem',
    borderLeftStyle: 'solid' as const,
    borderLeftColor: 'rgba(255,255,255,0.1)',
  },
  
  // Game: .money-field_bzk with .estimate-value_grH
  estimateValue: {
    flex: '1',
    textAlign: 'right' as const,
    fontSize: 'var(--fontSizeM)',
    fontWeight: 'normal',
    color: '#8bdb46', // positiveColor - green for income
    backgroundColor: 'rgba(6,10,16,0.4)',
    padding: '5rem 10rem',
    borderRadius: '8rem',
    whiteSpace: 'nowrap' as const,
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
  },
  
  // Money sign (Game: .sign__wn)
  moneySign: {
    marginRight: '0.25em',
  },
  
  // Money content (Game: .content_pa5)
  moneyContent: {
    flex: '1',
    textAlign: 'right' as const,
    whiteSpace: 'nowrap' as const,
  },
  
  // Prefab list (expanded content)
  prefabList: {
    // No background - keeps consistency with game's style
  },
  
  // Prefab row (sub-items) - Game: taxation-item_p0I
  prefabRow: {
    minHeight: '48rem',
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
    transition: 'background-color 0.15s ease',
    borderTopWidth: '2rem',
    borderTopStyle: 'solid' as const,
    borderTopColor: 'rgba(255,255,255,0.1)',
  },
  
  prefabRowHover: {
    backgroundColor: 'rgba(255, 255, 255, 0.02)',
  },
  
  // Game: .icon_Eeh{width:32rem;height:32rem;margin:4rem 12rem 4rem 32rem}
  prefabIcon: {
    width: '32rem',
    height: '32rem',
    margin: '4rem 12rem 4rem 32rem',
    flexShrink: 0,
  },
  
  // Game: .title_EA9{flex:1;margin:4rem 0;font-size:var(--fontSizeM);color:var(--textColor)}
  prefabName: {
    flex: '1',
    margin: '4rem 0',
    color: 'var(--textColor)',
    fontSize: 'var(--fontSizeM)',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap' as const,
  },
  
  // HIDDEN (percentual removido conforme solicitado)
  prefabRateValue: {
    display: 'none',
  },
  
  // Prefab tax slider wrapper (Game: tax-slider_HNg slider_rbN)
  prefabTaxSlider: {
    width: '580rem',
    alignSelf: 'stretch',
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'stretch',
  },

  // Prefab slider column (contains rate + slider)
  prefabSliderColumn: {
    flex: '1',
    borderLeftWidth: '2rem',
    borderLeftStyle: 'solid' as const,
    borderLeftColor: 'rgba(255,255,255,0.1)',
    padding: '4rem 22rem 4rem 12rem',
  },
  
  // Prefab slider wrapper (Game: wrapper_TUT + slider_lZg)
  prefabSliderWrapper: {
    flex: '1',
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
  },
  
  // Prefab estimate column wrapper
  prefabEstimateColumn: {
    width: '166rem',
    padding: '6rem 12rem',
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
    borderLeftWidth: '2rem',
    borderLeftStyle: 'solid' as const,
    borderLeftColor: 'rgba(255,255,255,0.1)',
  },
  
  prefabEstimateValue: {
    flex: '1',
    textAlign: 'right' as const,
    fontSize: 'var(--fontSizeS)',
    fontWeight: 'normal',
    color: '#8bdb46', // positiveColor - green for income
    backgroundColor: 'rgba(6,10,16,0.4)',
    padding: '5rem 10rem',
    borderRadius: '8rem',
    whiteSpace: 'nowrap' as const,
    display: 'flex',
    flexDirection: 'row' as const,
    alignItems: 'center',
  },
  
  loadingText: {
    textAlign: 'center' as const,
    padding: '40rem',
    color: 'var(--textColorDim)',
    fontSize: '14rem',
  },
  
  emptyState: {
    textAlign: 'center' as const,
    padding: '20rem',
    color: 'var(--textColorDim)',
    fontSize: '13rem',
    fontStyle: 'italic' as const,
  },

  footer: {
    padding: '10rem 16rem',
    borderTopWidth: '2rem',
    borderTopStyle: 'solid' as const,
    borderTopColor: 'rgba(255,255,255,0.1)',
    display: 'flex',
    justifyContent: 'flex-end',
  },

  applyBtn: {
    backgroundColor: 'var(--accentColorNormal)',
    color: 'var(--textColor)',
    border: 'none',
    borderRadius: '4rem',
    padding: '6rem 16rem',
    fontSize: '12rem',
    cursor: 'pointer',
  },

  applyBtnHover: {
    backgroundColor: 'var(--accentColorNormal-hover)',
  },

  applyBtnDisabled: {
    opacity: 0.6,
    cursor: 'not-allowed',
  },
};

import { getIconPath } from '../utils/icon';

// Helper to get prefab name for display
const getPrefabDisplayName = (prefab: { name: string; displayName: string; thumbnail?: string }): string => {
  // Use displayName if available (now comes from game localization), otherwise use internal name
  return prefab.displayName && prefab.displayName.trim() !== '' ? prefab.displayName : prefab.name;
};

// Capitalize first letter and lowercase the rest for single-word category types
const capitalizeType = (s: string | undefined | null): string => {
  if (!s) return s || '';
  return s.charAt(0).toUpperCase() + s.slice(1).toLowerCase();
};

// Category Row Component - matches game's taxation row layout
// Game structure: taxation-group_aQb contains header_cPd and content_NUa
// Border-top is on the GROUP (taxation-group_aQb+.taxation-group_aQb), not on internal elements
const CategoryRow: React.FC<{
  category: { type: string; displayName?: string; defaultFee: number; prefabs: Array<{ name: string; displayName: string; fee: number }> };
  isFirst?: boolean;
  onCategoryFeeChange: (newFee: number) => void;
  onPrefabFeeChange: (prefabName: string, newFee: number) => void;
}> = ({ category, isFirst = false, onCategoryFeeChange, onPrefabFeeChange }) => {
  const [expanded, setExpanded] = useState(false);
  const [headerHover, setHeaderHover] = useState(false);
  const [buttonHover, setButtonHover] = useState(false);
  
  const iconUrl = getIconPath(((category as any).icon) || category.type);
  const hasPrefabs = category.prefabs && category.prefabs.length > 0;
  
  // Game: taxation-group_aQb + .taxation-group_aQb { border-top }
  // The border goes on the GROUP container itself, not on internal elements
  const groupStyle = isFirst ? S.categoryGroupBase : S.categoryGroupWithBorder;
  
  // Calcular o range dos itens
  const fees = category.prefabs.map(p => p.fee);
  const minFee = fees.length > 0 ? Math.min(...fees) : 0;
  const maxFee = fees.length > 0 ? Math.max(...fees) : 0;
  let feeRange = `¢ ${minFee}`;
  if (fees.length > 1 && minFee !== maxFee) {
    feeRange = `¢ ${minFee} - ¢ ${maxFee}`;
  }

  // Tornar o campo rateValue visível
  const rateValueStyle = { ...S.rateValue, display: 'block', color: 'var(--textColorDim)', fontWeight: 500, fontSize: '13rem', minWidth: '80rem', textAlign: 'left' as const };

  return (
    <div style={groupStyle}>
      {/* Category Header Row - Game: header_cPd item-hover_WK8 item-focused_FuT */}
      <div 
        style={{
          ...S.categoryHeader,
          ...(headerHover ? S.categoryHeaderHover : {}),
        }}
        onMouseEnter={() => setHeaderHover(true)}
        onMouseLeave={() => setHeaderHover(false)}
      >
        {/* Icon - Game: icon_vPX */}
        <img src={iconUrl} style={S.categoryIcon} onError={(e) => { (e.currentTarget as HTMLImageElement).src = getIconPath(undefined); }} />
        
        {/* Title - Game: title_WQ0 */}
        <div style={S.categoryTitle}>{category.displayName || capitalizeType(category.type)}</div>
        
        {/* Expand Button - Game: button_bvQ + toggle-button_hTU */}
        {hasPrefabs && (
          <div 
            style={{
              ...S.expandButton,
              ...(buttonHover ? S.expandButtonHover : {}),
            }}
            onClick={(e) => {
              e.stopPropagation();
              setExpanded(!expanded);
            }}
            onMouseEnter={() => setButtonHover(true)}
            onMouseLeave={() => setButtonHover(false)}
          >
            <div 
              style={{
                ...S.expandIcon,
                ...(expanded ? S.expandIconExpanded : {}),
              }}
            />
          </div>
        )}
        
        {/* Tax Slider Container - Game: tax-slider_HNg header-slider_wLI */}
        <div style={S.taxSliderContainer} onClick={(e) => e.stopPropagation()}>
          {/* Slider Column - Game: slider-column_XmW (contains rate + slider) */}
          <div style={S.sliderColumn}>
            {/* Range dos itens no campo rateValue */}
            <div style={rateValueStyle}>{feeRange}</div>
            {/* Custom Slider - Game: wrapper_TUT + slider_lZg */}
            <CustomSlider
              value={category.defaultFee}
              min={0}
              max={50}
              onChange={(v) => onCategoryFeeChange(v)}
            />
          </div>
          
          {/* Estimate Column - Game: estimate-column_DQB */}
          <div style={S.estimateColumn}>
            {/* Money Field - Game: money-field_bzk */}
            <div style={S.estimateValue}>
              <div style={S.moneySign}>¢</div>
              <div style={S.moneyContent}>{category.defaultFee}</div>
            </div>
          </div>
        </div>
      </div>
      
      {/* Expanded Prefab List - Game: content_NUa */}
      {expanded && hasPrefabs && (
        <div style={S.prefabList}>
          {category.prefabs.map(prefab => (
            <PrefabRow
              key={prefab.name}
              prefab={prefab}
              onFeeChange={(newFee) => onPrefabFeeChange(prefab.name, newFee)}
            />
          ))}
        </div>
      )}
    </div>
  );
};

// Prefab Row Component - sub-item style
const PrefabRow: React.FC<{
  prefab: { name: string; displayName: string; thumbnail?: string; fee: number };
  onFeeChange: (newFee: number) => void;
}> = ({ prefab, onFeeChange }) => {
  const [hover, setHover] = useState(false);
  
  return (
    <div 
      style={{
        ...S.prefabRow,
        ...(hover ? S.prefabRowHover : {}),
      }}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
    >
      {/* Prefab Icon */}
      <img src={prefab.thumbnail || "Media/Game/Icons/Parking.svg"} style={S.prefabIcon} />
      
      {/* Prefab Name */}
      <div style={S.prefabName}>{getPrefabDisplayName(prefab)}</div>
      
      {/* Prefab Tax Slider - Game: tax-slider_HNg slider_rbN */}
      <div style={S.prefabTaxSlider}>
        {/* Slider Column - contains rate + slider */}
        <div style={S.prefabSliderColumn}>
          {/* Rate Value */}
          <div style={S.prefabRateValue}>{prefab.fee} %</div>
          
          {/* Custom Slider - Game: wrapper_TUT + slider_lZg */}
          <CustomSlider
            value={prefab.fee}
            min={0}
            max={50}
            onChange={(v) => onFeeChange(v)}
          />
        </div>
        
        {/* Estimate Column */}
        <div style={S.prefabEstimateColumn}>
          {/* Money Field */}
          <div style={S.prefabEstimateValue}>
            <div style={S.moneySign}>¢</div>
            <div style={S.moneyContent}>{prefab.fee}</div>
          </div>
        </div>
      </div>
    </div>
  );
};

export const ParkingFeePanel: React.FC<ParkingFeePanelProps> = ({ onClose }) => {
  useRefreshOnOpen();
  const config = useValue(config$);
  const { translate } = useLocalization();
  const [hasChanges, setHasChanges] = useState(false);
  const [applyHover, setApplyHover] = useState(false);
  const panelTitle = translate('ParkingFeeControl[PanelTitle]', 'Parking Fees') || 'Parking Fees';
  const columnType = translate('ParkingFeeControl[Type]', 'Type') || 'Type';
  const columnFeeRate = translate('ParkingFeeControl[FeeRate]', 'Fee Rate') || 'Fee Rate';
  const columnFee = translate('ParkingFeeControl[Fee]', 'Fee') || 'Fee';
  const applyNowLabel = translate('ParkingFeeControl[ApplyNow]', 'Apply Now') || 'Apply Now';

  useEffect(() => {
    setHasChanges(false);
  }, []);

  const getCategoryDisplayName = (type: string) => {
    const key = `ParkingFeeControl[${capitalizeType(type)}]`;
    return translate(key, capitalizeType(type)) || capitalizeType(type);
  };
  
  const handleCategoryFeeChange = (categoryType: string, newFee: number) => {
    const update: CategoryFeeUpdate = { categoryType, newFee };
    trigger('parkingfee', 'updateCategoryFee', update);
    setHasChanges(true);
  };
  
  const handlePrefabFeeChange = (categoryType: string, prefabName: string, newFee: number) => {
    const update: PrefabFeeUpdate = { categoryType, prefabName, newFee };
    trigger('parkingfee', 'updatePrefabFee', update);
    setHasChanges(true);
  };
  
  const handleApplyNow = () => {
    if (!hasChanges) return;
    trigger('parkingfee', 'applyNow');
    setHasChanges(false);
  };
  
  return (
    <div style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 9999,
      pointerEvents: 'auto',
    }}>
      <Panel
        header={panelTitle}
        onClose={onClose}
        style={{
          width: '1000rem',
          height: '680rem',
          backgroundColor: 'var(--panelColorNormal)',
          backdropFilter: 'var(--panelBlur)',
          borderRadius: '4rem',
          overflow: 'hidden',
          display: 'flex',
          flexDirection: 'column',
          boxShadow: '0 8rem 32rem rgba(0, 0, 0, 0.5)',
        }}
      >
      {/* Content with table section wrapper */}
      <div style={S.content}>
        <div style={S.tableSection}>
          {/* Column Headers - Game: header_l0j */}
          <div style={S.tableHeader}>
            <div style={S.columnHeaders}>
              <div style={S.columnType}>{columnType}</div>
              <div style={S.columnFeeRate}>{columnFeeRate}</div>
              <div style={S.columnEstimate}>{columnFee}</div>
            </div>
          </div>
          
          {/* Table Content Wrapper - Game: content_flM content_owQ */}
          {/* Contains scrollable area + table-lines overlay */}
          <div style={S.tableContentWrapper}>
            {/* Scrollable content - Game: scrollable_Xqt */}
            <Scrollable style={S.tableContent}>
              {config?.categories && config.categories.length > 0 ? (
                config.categories.map((category, index) => (
                  <CategoryRow
                    key={category.type}
                    category={{ ...category, displayName: getCategoryDisplayName(category.type) }}
                    isFirst={index === 0}
                    onCategoryFeeChange={(newFee) => handleCategoryFeeChange(category.type, newFee)}
                    onPrefabFeeChange={(prefabName, newFee) => 
                      handlePrefabFeeChange(category.type, prefabName, newFee)
                    }
                  />
                ))
              ) : (
                <div style={S.loadingText}>Loading configuration...</div>
              )}
            </Scrollable>
            
            {/* Table lines overlay - Game: table-lines_Abv */}
            {/* This creates the continuous vertical lines and bottom border */}
            <div style={S.tableLines}>
              <div style={S.tableLinesName}></div>
              <div style={S.tableLinesRate}></div>
              <div style={S.tableLinesEstimate}></div>
            </div>
          </div>
        </div>
      </div>
      
      <div style={S.footer}>
        <button
          style={{
            ...S.applyBtn,
            ...(applyHover && hasChanges ? S.applyBtnHover : {}),
            ...(!hasChanges ? S.applyBtnDisabled : {}),
          }}
          onClick={handleApplyNow}
          onMouseEnter={() => hasChanges && setApplyHover(true)}
          onMouseLeave={() => setApplyHover(false)}
          disabled={!hasChanges}
        >
          {applyNowLabel}
        </button>
      </div>
    </Panel>
    </div>
  );
};
