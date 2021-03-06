﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RdtClient.Data.Enums;
using RdtClient.Data.Models.Data;

namespace RdtClient.Data.Data
{
    public interface ITorrentData
    {
        Task<IList<Torrent>> Get();
        Task<Torrent> GetById(Guid id);
        Task<Torrent> GetByHash(String hash);
        Task<Torrent> Add(String realDebridId, String hash, Boolean autoDownload, Boolean autoDelete);
        Task UpdateRdData(Torrent torrent);
        Task UpdateStatus(Guid torrentId, TorrentStatus status);
        Task UpdateCategory(Guid torrentId, String category);
        Task Delete(Guid id);
    }

    public class TorrentData : ITorrentData
    {
        private readonly DataContext _dataContext;

        public TorrentData(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<Torrent>> Get()
        {
            var results = await _dataContext.Torrents
                                            .AsNoTracking()
                                            .Include(m => m.Downloads)
                                            .ToListAsync();

            foreach (var file in results.SelectMany(torrent => torrent.Downloads))
            {
                file.Torrent = null;
            }

            return results;
        }

        public async Task<Torrent> GetById(Guid id)
        {
            var dbTorrent = await _dataContext.Torrents
                                              .AsNoTracking()
                                              .Include(m => m.Downloads)
                                              .FirstOrDefaultAsync(m => m.TorrentId == id);

            if (dbTorrent == null)
            {
                return null;
            }

            foreach (var file in dbTorrent.Downloads)
            {
                file.Torrent = null;
            }

            return dbTorrent;
        }

        public async Task<Torrent> GetByHash(String hash)
        {
            var dbTorrent = await _dataContext.Torrents
                                              .AsNoTracking()
                                              .Include(m => m.Downloads)
                                              .FirstOrDefaultAsync(m => m.Hash.ToLower() == hash.ToLower());

            if (dbTorrent == null)
            {
                return null;
            }

            foreach (var file in dbTorrent.Downloads)
            {
                file.Torrent = null;
            }

            return dbTorrent;
        }

        public async Task<Torrent> Add(String realDebridId, String hash, Boolean autoDownload, Boolean autoDelete)
        {
            var torrent = new Torrent
            {
                TorrentId = Guid.NewGuid(),
                RdId = realDebridId,
                Hash = hash.ToLower(),
                Status = TorrentStatus.RealDebrid,
                AutoDownload = autoDownload,
                AutoDelete = autoDelete
            };

            await _dataContext.Torrents.AddAsync(torrent);

            await _dataContext.SaveChangesAsync();

            return torrent;
        }

        public async Task UpdateRdData(Torrent torrent)
        {
            var dbTorrent = await _dataContext.Torrents.FirstOrDefaultAsync(m => m.TorrentId == torrent.TorrentId);

            if (dbTorrent == null)
            {
                return;
            }

            dbTorrent.RdName = torrent.RdName;
            dbTorrent.RdSize = torrent.RdSize;
            dbTorrent.RdHost = torrent.RdHost;
            dbTorrent.RdSplit = torrent.RdSplit;
            dbTorrent.RdProgress = torrent.RdProgress;
            dbTorrent.RdStatus = torrent.RdStatus;
            dbTorrent.RdAdded = torrent.RdAdded;
            dbTorrent.RdEnded = torrent.RdEnded;
            dbTorrent.RdSpeed = torrent.RdSpeed;
            dbTorrent.RdSeeders = torrent.RdSeeders;

            if (torrent.Files != null)
            {
                dbTorrent.RdFiles = torrent.RdFiles;
            }

            await _dataContext.SaveChangesAsync();
        }

        public async Task UpdateStatus(Guid torrentId, TorrentStatus status)
        {
            var dbTorrent = await _dataContext.Torrents.FirstOrDefaultAsync(m => m.TorrentId == torrentId);

            if (dbTorrent == null)
            {
                return;
            }

            dbTorrent.Status = status;

            await _dataContext.SaveChangesAsync();
        }

        public async Task UpdateCategory(Guid torrentId, String category)
        {
            var dbTorrent = await _dataContext.Torrents.FirstOrDefaultAsync(m => m.TorrentId == torrentId);

            if (dbTorrent == null)
            {
                return;
            }

            dbTorrent.Category = category;

            await _dataContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var dbTorrent = await _dataContext.Torrents.FirstOrDefaultAsync(m => m.TorrentId == id);

            if (dbTorrent == null)
            {
                return;
            }

            _dataContext.Torrents.Remove(dbTorrent);

            await _dataContext.SaveChangesAsync();
        }
    }
}