%global debug_package %{nil}
%global repo_root %{_topdir}/..
%global base_version %(echo "$(sed -ne '/Version/{s/.*<Version>\\(.*\\)<\\/Version>.*/\\1/p;q;}' < MuteBoi.csproj)")

%if %{defined dev_build}
Name:       muteboi-dev
Summary:    Retains specific Discord roles if users rejoin the server (dev build)
Version:    %{base_version}~%(date "+%%Y%%m%%d%%H%%M%%S")git%(git rev-parse --short HEAD)
Provides:   muteboi
%else
Name:       muteboi
Summary:    Retains specific Discord roles if users rejoin the server
Version:    %{base_version}
%endif
Release:    1%{?dist}
License:    GPLv3
URL:        https://github.com/KarlOfDuty/MuteBoi
Packager:   KarlofDuty
Source:     rpm-source.tar.gz

BuildRequires: systemd-rpm-macros
Requires: dotnet-runtime-9.0
Requires: mariadb-server
%{?systemd_requires}

%description
Retains specific Discord roles if users rejoin the server.
Useful for muted roles or other permission negating roles.
Leaving members are saved in a mysql database with all tracked roles they had when they left.

%prep
%setup -T -c

%build
dotnet publish %{repo_root}/MuteBoi.csproj -p:PublishSingleFile=true -r linux-x64 -c Release --self-contained false --output %{_builddir}/out

%install
if [[ -d %{_rpmdir}/%{_arch} ]]; then
  %{__rm} %{_rpmdir}/%{_arch}/*
fi

%{__install} -d %{buildroot}/usr/bin
# rpmbuild post-processing using the strip command breaks dotnet binaries, remove the executable bit to avoid it
%{__install} -m 644 %{_builddir}/out/muteboi %{buildroot}/usr/bin/muteboi

%{__install} -d %{buildroot}/usr/lib/systemd/system
%{__install} -m 644 %{repo_root}/packaging/muteboi.service %{buildroot}/usr/lib/systemd/system/

%{__install} -d %{buildroot}/etc/muteboi/
%{__install} -m 600 %{repo_root}/default_config.yml %{buildroot}/etc/muteboi/config.yml

%{__install} -d %{buildroot}/var/log/muteboi

%pre
getent group muteboi > /dev/null || groupadd muteboi
getent passwd muteboi > /dev/null || useradd -r -m -d /var/lib/muteboi -s /sbin/nologin -g muteboi muteboi

%post
SYSTEMD_VERSION=$(systemctl --version | awk '{if($1=="systemd" && $2~"^[0-9]"){print $2}}' | head -n 1)
if (( $SYSTEMD_VERSION < 253 )); then
    echo "Systemd version is lower than 253 ($SYSTEMD_VERSION); using legacy service type 'notify' instead of 'notify-reload'"
    sed -i 's/^Type=notify-reload$/Type=notify/' "/usr/lib/systemd/system/muteboi.service"
fi
%systemd_post muteboi.service

%preun
%systemd_preun muteboi.service

%postun
%systemd_postun_with_restart muteboi.service

%files
%attr(0755,root,root) /usr/bin/muteboi
%attr(0644,root,root) /usr/lib/systemd/system/muteboi.service
%config %attr(0600, muteboi, muteboi) /etc/muteboi/config.yml
%dir %attr(0700, muteboi, muteboi) /var/lib/muteboi
%dir %attr(0755, muteboi, muteboi) /var/log/muteboi