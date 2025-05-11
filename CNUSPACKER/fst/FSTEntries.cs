using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using CNUSPACKER.Packaging;

namespace CNUSPACKER.FST
{
    /// <summary>
    /// Manages a tree of <see cref="FSTEntry"/> instances and provides serialization logic.
    /// </summary>
    public class FSTEntries
    {
        private readonly List<FSTEntry> _entries;
        private readonly ILogger<FSTEntries>? _logger;
        private readonly Packaging.FST _fst;

        /// <summary>
        /// Initializes a new <see cref="FSTEntries"/> tree with a root node.
        /// </summary>
        public FSTEntries(Packaging.FST fst, ILogger<FSTEntries>? logger = null)
        {
            _fst = fst;
            _logger = logger;
            _entries = new List<FSTEntry> { new FSTEntry(fst) };
            _logger?.LogDebug("Initialized FSTEntries with root entry.");
        }



        /// <summary>
        /// Updates all entries and recalculates offsets.
        /// </summary>
        public void Update()
        {
            foreach (var entry in _entries)
            {
                entry.Update();
            }
            UpdateDirRefs();
        }

        private void UpdateDirRefs()
        {
            if (_entries.Count == 0) return;

            var root = _entries[0];
            root.ParentOffset = 0;
            root.NextOffset = _fst.CurEntryOffset;

            var lastDir = root.UpdateDirRefs();
            if (lastDir != null)
            {
                lastDir.NextOffset = _fst.CurEntryOffset;
                _logger?.LogDebug("Set nextOffset on last directory to curEntryOffset ({Offset})", _fst.CurEntryOffset);
            }
        }

        /// <summary>
        /// Returns all <see cref="FSTEntry"/> objects belonging to a specific content block.
        /// </summary>
        public List<FSTEntry> GetFSTEntriesByContent(Content content)
        {
            return _entries.SelectMany(e => e.GetFSTEntriesByContent(content)).ToList();
        }

        /// <summary>
        /// Gets the total count of FST entries in the tree.
        /// </summary>
        public int GetFSTEntryCount()
        {
            return _entries.Sum(e => e.GetEntryCount());
        }

        /// <summary>
        /// Serializes all entries into a flat byte array.
        /// </summary>
        public byte[] GetAsData()
        {
            return _entries.SelectMany(e => e.GetAsData()).ToArray();
        }

        /// <summary>
        /// Gets the byte size of the full serialized FST entry block.
        /// </summary>
        public int GetDataSize()
        {
            return GetFSTEntryCount() * 0x10;
        }

        /// <summary>
        /// Gets the root entry.
        /// </summary>
        public FSTEntry GetRootEntry()
        {
            return _entries[0];
        }
    }
}