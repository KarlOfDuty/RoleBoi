%global debug_package %{nil}
%global repo_root %{_topdir}/..
%global base_version %(echo "$(sed -ne '/Version/{s/.*<Version>\\(.*\\)<\\/Version>.*/\\1/p;q;}' < RoleBoi.csproj)")

%if %{defined dev_build}
Name:       roleboi-dev
Summary:    Retains specific Discord roles if users rejoin the server (dev build)
Version:    %{base_version}~%(date "+%%Y%%m%%d%%H%%M%%S")git%(git rev-parse --short HEAD)
Provides:   roleboi
%else
Name:       roleboi
Summary:    Retains specific Discord roles if users rejoin the server
Version:    %{base_version}
%endif
Release:    1%{?dist}
License:    GPLv3
URL:        https://github.com/KarlOfDuty/RoleBoi
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
dotnet publish %{repo_root}/RoleBoi.csproj -p:PublishSingleFile=true -r linux-x64 -c Release --self-contained false --output %{_builddir}/out

%install
if [[ -d %{_rpmdir}/%{_arch} ]]; then
  %{__rm} %{_rpmdir}/%{_arch}/*
fi

%{__install} -d %{buildroot}/usr/bin
# rpmbuild post-processing using the strip command breaks dotnet binaries, remove the executable bit to avoid it
%{__install} -m 644 %{_builddir}/out/roleboi %{buildroot}/usr/bin/roleboi

%{__install} -d %{buildroot}/usr/lib/systemd/system
%{__install} -m 644 %{repo_root}/packaging/roleboi.service %{buildroot}/usr/lib/systemd/system/

%{__install} -d %{buildroot}/etc/roleboi/
%{__install} -m 600 %{repo_root}/default_config.yml %{buildroot}/etc/roleboi/config.yml

%{__install} -d %{buildroot}/var/lib/roleboi
%{__install} -d %{buildroot}/var/log/roleboi

%pre
getent group roleboi > /dev/null || groupadd roleboi
getent passwd roleboi > /dev/null || useradd -r -m -d /var/lib/roleboi -s /sbin/nologin -g roleboi roleboi

%post
SYSTEMD_VERSION=$(systemctl --version | awk '{if($1=="systemd" && $2~"^[0-9]"){print $2}}' | head -n 1)
if (( $SYSTEMD_VERSION < 253 )); then
    echo "Systemd version is lower than 253 ($SYSTEMD_VERSION); using legacy service type 'notify' instead of 'notify-reload'"
    sed -i 's/^Type=notify-reload$/Type=notify/' "/usr/lib/systemd/system/roleboi.service"
fi
%systemd_post roleboi.service

%preun
%systemd_preun roleboi.service

%postun
%systemd_postun_with_restart roleboi.service

%files
%attr(0755,root,root) /usr/bin/roleboi
%attr(0644,root,root) /usr/lib/systemd/system/roleboi.service
%config %attr(0600, roleboi, roleboi) /etc/roleboi/config.yml
%dir %attr(0700, roleboi, roleboi) /var/lib/roleboi
%dir %attr(0755, roleboi, roleboi) /var/log/roleboi