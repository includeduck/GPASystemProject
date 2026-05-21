import { useEffect, useState } from 'react';
import { Award, Plus, Save, Sliders, Trash2, ShieldAlert } from 'lucide-react';
import { StatusBanner } from '../components/StatusBanner';
import { EmptyState } from '../components/EmptyState';
import { gradingPolicyApi, getApiErrorMessage } from '../services/api';
import type { UpdateGradingPolicyRequest } from '../types/models';

export function GradingPolicyPage() {
  const [policies, setPolicies] = useState<UpdateGradingPolicyRequest[]>([]);
  const [cutoff, setCutoff] = useState<number>(50);
  const [loading, setLoading] = useState(true);
  const [savingPolicies, setSavingPolicies] = useState(false);
  const [savingCutoff, setSavingCutoff] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Validation messages
  const [validationErrors, setValidationErrors] = useState<string[]>([]);

  const loadData = async () => {
    setLoading(true);
    try {
      const policyList = await gradingPolicyApi.list();
      setPolicies(
        policyList.map((p) => ({
          policyId: p.policyId,
          letterGrade: p.letterGrade,
          minPercentage: p.minPercentage,
          maxPercentage: p.maxPercentage,
          gradePoint: p.gradePoint,
          isActive: p.isActive,
          effectiveFrom: p.effectiveFrom,
        }))
      );

      const config = await gradingPolicyApi.getConfig();
      setCutoff(config.pass_fail_cutoff);
      setError(null);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, []);

  // Recalculate validation errors on policies change
  useEffect(() => {
    if (policies.length === 0) {
      setValidationErrors(['At least one grading policy is required.']);
      return;
    }

    const errors: string[] = [];
    const sorted = [...policies].sort((a, b) => a.minPercentage - b.minPercentage);

    // 1. Minimum of first range must be 0
    if (sorted[0].minPercentage !== 0) {
      errors.push(`First range must start at 0% (currently starts at ${sorted[0].minPercentage}%).`);
    }

    // 2. Maximum of last range must be 100
    if (sorted[sorted.length - 1].maxPercentage !== 100) {
      errors.push(`Last range must end at 100% (currently ends at ${sorted[sorted.length - 1].maxPercentage}%).`);
    }

    // 3. Contiguous and non-overlapping check
    for (let i = 0; i < sorted.length; i++) {
      const p = sorted[i];
      if (p.minPercentage >= p.maxPercentage) {
        errors.push(`Grade "${p.letterGrade || 'Unnamed'}": Min (${p.minPercentage}%) must be less than Max (${p.maxPercentage}%).`);
      }
      if (p.gradePoint < 0 || p.gradePoint > 4.33) {
        errors.push(`Grade "${p.letterGrade || 'Unnamed'}": Grade Point must be between 0.00 and 4.33.`);
      }
      if (!p.letterGrade.trim()) {
        errors.push('Letter grade cannot be empty.');
      }

      if (i < sorted.length - 1) {
        const next = sorted[i + 1];
        if (p.maxPercentage !== next.minPercentage) {
          errors.push(
            `Gap or overlap detected between "${p.letterGrade}" (ends at ${p.maxPercentage}%) and "${next.letterGrade}" (starts at ${next.minPercentage}%).`
          );
        }
      }
    }

    setValidationErrors(errors);
  }, [policies]);

  const handlePolicyChange = (index: number, field: keyof UpdateGradingPolicyRequest, value: unknown) => {
    const updated = [...policies];
    updated[index] = {
      ...updated[index],
      [field]: value,
    };
    setPolicies(updated);
  };

  const handleAddRow = () => {
    const lastMax = policies.length > 0 ? Math.max(...policies.map((p) => p.maxPercentage)) : 0;
    const newPolicy: UpdateGradingPolicyRequest = {
      letterGrade: '',
      minPercentage: lastMax < 100 ? lastMax : 0,
      maxPercentage: lastMax < 90 ? lastMax + 10 : 100,
      gradePoint: 0,
      isActive: true,
      effectiveFrom: new Date().toISOString().split('T')[0],
    };
    setPolicies([...policies, newPolicy]);
  };

  const handleRemoveRow = (index: number) => {
    setPolicies(policies.filter((_, i) => i !== index));
  };

  const handleSavePolicies = async () => {
    if (validationErrors.length > 0) return;
    setSavingPolicies(true);
    setError(null);
    setSuccess(null);
    try {
      const updated = await gradingPolicyApi.update(policies);
      setPolicies(
        updated.map((p) => ({
          policyId: p.policyId,
          letterGrade: p.letterGrade,
          minPercentage: p.minPercentage,
          maxPercentage: p.maxPercentage,
          gradePoint: p.gradePoint,
          isActive: p.isActive,
          effectiveFrom: p.effectiveFrom,
        }))
      );
      setSuccess('Grading policies updated successfully.');
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSavingPolicies(false);
    }
  };

  const handleSaveCutoff = async () => {
    if (cutoff < 0 || cutoff > 100) {
      setError('Cutoff must be between 0 and 100.');
      return;
    }
    setSavingCutoff(true);
    setError(null);
    setSuccess(null);
    try {
      await gradingPolicyApi.updateConfig(cutoff);
      setSuccess('Passing threshold cutoff updated.');
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSavingCutoff(false);
    }
  };

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Grading Policies</h1>
          <p>Configure letter grade thresholds and academic standards</p>
        </div>
      </div>

      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}

      <div className="form-panel" style={{ marginBottom: '2rem' }}>
        <div className="form-panel__header">
          <h2 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <Sliders size={20} /> Passing Threshold Configuration
          </h2>
        </div>
        <div className="form-grid form-grid--two">
          <label>
            <span>Pass/Fail Cutoff Percentage (%)</span>
            <input
              type="number"
              min={0}
              max={100}
              step={0.1}
              required
              value={cutoff}
              onChange={(e) => setCutoff(parseFloat(e.target.value) || 0)}
            />
            <small style={{ color: 'var(--text-secondary)', marginTop: '0.25rem', display: 'block' }}>
              Final course percentages below this value automatically receive an F (0.00 GP).
            </small>
          </label>
        </div>
        <div className="form-actions">
          <button
            className="button button--primary"
            type="button"
            disabled={savingCutoff}
            onClick={handleSaveCutoff}
          >
            <Save size={17} /> Save Cutoff
          </button>
        </div>
      </div>

      <div className="table-panel">
        <div
          className="form-panel__header"
          style={{ padding: '1.25rem 1.5rem', borderBottom: '1px solid var(--border)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
        >
          <h2 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <Award size={20} /> Grade Interval Configurations
          </h2>
          <button className="button button--secondary" onClick={handleAddRow} type="button">
            <Plus size={17} /> Add Interval
          </button>
        </div>

        {/* Validation visual alerts banner */}
        {validationErrors.length > 0 && (
          <div
            style={{
              margin: '1.5rem',
              padding: '1rem 1.25rem',
              backgroundColor: 'rgba(239, 68, 68, 0.08)',
              borderLeft: '4px solid #ef4444',
              borderRadius: '6px',
            }}
          >
            <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
              <ShieldAlert style={{ color: '#ef4444', flexShrink: 0, marginTop: '2px' }} size={18} />
              <div>
                <h4 style={{ color: '#ef4444', margin: 0, fontWeight: 600 }}>Validation Errors Detected</h4>
                <ul style={{ margin: '0.5rem 0 0', paddingLeft: '1.25rem', fontSize: '0.9rem', color: '#b91c1c' }}>
                  {validationErrors.map((err, idx) => (
                    <li key={idx}>{err}</li>
                  ))}
                </ul>
                <p style={{ margin: '0.5rem 0 0 0', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Ranges must be sorted chronologically and cover 0% to 100% contiguously to be saved.
                </p>
              </div>
            </div>
          </div>
        )}

        {loading ? (
          <EmptyState title="Loading grading policies..." />
        ) : policies.length === 0 ? (
          <EmptyState title="No grading policy intervals defined" detail="Click Add Interval to create one." />
        ) : (
          <div className="table-wrap" style={{ padding: '0 1.5rem 1.5rem' }}>
            <table>
              <thead>
                <tr>
                  <th style={{ width: '20%' }}>Letter Grade</th>
                  <th style={{ width: '25%' }}>Min Percentage (%)</th>
                  <th style={{ width: '25%' }}>Max Percentage (%)</th>
                  <th style={{ width: '20%' }}>Grade Point (GP)</th>
                  <th style={{ width: '10%' }} className="table-actions">Remove</th>
                </tr>
              </thead>
              <tbody>
                {policies.map((policy, index) => (
                  <tr key={index}>
                    <td>
                      <input
                        type="text"
                        maxLength={3}
                        required
                        placeholder="e.g. A"
                        value={policy.letterGrade}
                        style={{ width: '100%', padding: '0.375rem 0.75rem', textTransform: 'uppercase' }}
                        onChange={(e) => handlePolicyChange(index, 'letterGrade', e.target.value.toUpperCase())}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        max={100}
                        step={0.1}
                        required
                        value={policy.minPercentage}
                        style={{ width: '100%', padding: '0.375rem 0.75rem' }}
                        onChange={(e) => handlePolicyChange(index, 'minPercentage', parseFloat(e.target.value) || 0)}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0}
                        max={100}
                        step={0.1}
                        required
                        value={policy.maxPercentage}
                        style={{ width: '100%', padding: '0.375rem 0.75rem' }}
                        onChange={(e) => handlePolicyChange(index, 'maxPercentage', parseFloat(e.target.value) || 0)}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min={0.0}
                        max={4.33}
                        step={0.01}
                        required
                        value={policy.gradePoint}
                        style={{ width: '100%', padding: '0.375rem 0.75rem' }}
                        onChange={(e) => handlePolicyChange(index, 'gradePoint', parseFloat(e.target.value) || 0)}
                      />
                    </td>
                    <td className="table-actions">
                      <button
                        className="icon-button icon-button--danger"
                        type="button"
                        onClick={() => handleRemoveRow(index)}
                        aria-label="Remove interval"
                      >
                        <Trash2 size={17} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="form-actions" style={{ marginTop: '1.5rem', padding: 0 }}>
              <button
                className="button button--primary"
                type="button"
                disabled={savingPolicies || validationErrors.length > 0}
                onClick={handleSavePolicies}
              >
                <Save size={17} /> Save Policy Configuration
              </button>
            </div>
          </div>
        )}
      </div>
    </section>
  );
}
